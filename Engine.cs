using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using System;
using System.IO;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using TheAdventure.Models;
using TheAdventure.Models.Data;
using CSCore.Codecs;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        // Add smoke sound
        private readonly IWaveSource _soundSource;
        private readonly ISoundOut _soundOut;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);

            // Load the sound file
            var soundPath = Path.Combine("Assets", "smoke_sound1.wav");
            _soundSource = CodecFactory.Instance.GetCodec(soundPath)
                             .ToSampleSource()
                             .ToWaveSource();

            // Create the sound output device
            _soundOut = new WasapiOut();
        }

        public void InitializeWorld()
        {
            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, jsonSerializerOptions);
            if (level == null) return;
            foreach (var refTileSet in level.TileSets)
            {
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if (!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet))
                {
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, jsonSerializerOptions);

                    foreach (var tile in tileSet.Tiles)
                    {
                        var internalTextureId = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                        tile.InternalTextureId = internalTextureId;
                    }

                    _loadedTileSets[refTileSet.Source] = tileSet;
                }

                refTileSet.Set = tileSet;
            }

            _currentLevel = level;
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();

            // Adding a new variable to check if SpaceBar is pressed
            bool IsSpaceBar = _input.IsSpaceBarPressed();

            // Adding a new variable to check if Tab is pressed
            bool IsTab = _input.IsTabPressed();

            if (IsSpaceBar)
            {
                AddSmokeEffect(_player.Position.X, _player.Position.Y, true);
                //PlaySmokeSound();
            }

            // Check if Tab is pressed, if so, triple the speed to make the player run
            if (IsTab)
            {
                _player.IncreaseSpeed(3.0);
            }
            else
            {
                // Otherwise, reset speed at the default value
                _player.ResetSpeed();
            }

            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            _renderer.PresentFrame();
        }

        private Tile? GetTile(int id)
        {
            if (_currentLevel == null) return null;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Set.Tiles)
                {
                    if (tile.Id == id)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        private void RenderTerrain()
        {
            if (_currentLevel == null) return;
            for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
            {
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j)
                    {
                        var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                        var cTile = GetTile(cTileId);
                        if (cTile == null) continue;

                        var src = new Rectangle<int>(0, 0, cTile.ImageWidth, cTile.ImageHeight);
                        var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight, cTile.ImageWidth,
                            cTile.ImageHeight);

                        _renderer.RenderTexture(cTile.InternalTextureId, src, dst);
                    }
                }
            }
        }

        private IEnumerable<RenderableGameObject> GetAllRenderableObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject renderableGameObject)
                {
                    yield return renderableGameObject;
                }
            }
        }

        private IEnumerable<TemporaryGameObject> GetAllTemporaryGameObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject temporaryGameObject)
                {
                    yield return temporaryGameObject;
                }
            }
        }

        private void RenderAllObjects()
        {
            foreach (var gameObject in GetAllRenderableObjects())
            {
                gameObject.Render(_renderer);
            }

            _player.Render(_renderer);
        }

        private void AddBomb(int x, int y)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }

        private void AddSmokeEffect(int x, int y, bool isPlayerPosition = false)
        {
            // Define offset variable to position smoke
            var offset = 16;

            var pos = isPlayerPosition ? (_player.Position.X, _player.Position.Y) : (x, y);

            // Load the sprite sheet for smoke.png
            var spriteSheet = SpriteSheet.LoadSpriteSheet("smoke.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                // Use the same animation as the bomb
                spriteSheet.ActivateAnimation("Explode");

                // Define the positions for the four smoke parts around the player
                var positions = new List<(int, int)>
                {
                    (pos.Item1 - offset, pos.Item2 - offset), // Top-left
                    (pos.Item1 + offset, pos.Item2 - offset), // Top-right
                    (pos.Item1 - offset, pos.Item2 + offset), // Bottom-left
                    (pos.Item1 + offset, pos.Item2 + offset)  // Bottom-right
                };

                // Create and add each smoke effect to the game objects dictionary
                foreach (var position in positions)
                {
                    TemporaryGameObject smokeEffect = new(spriteSheet, 2.1, position);
                    _gameObjects.Add(smokeEffect.Id, smokeEffect);
                }
            }

        }

        private void PlaySmokeSound()
        {
            // Set the source of the sound output
            _soundOut.Initialize(_soundSource);

            // Play the sound
            _soundOut.Play();

            // Wait until it ends
            _soundOut.WaitForStopped();

            // Dispose the sound output
            _soundOut.Dispose();
        }
    }
}