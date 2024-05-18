using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;
using System.IO;
using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;

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

        private TeleporterObject _teleporter1;
        private TeleporterObject _teleporter2;
        private IWavePlayer _waveOutEvent;
        private AudioFileReader _audioFileReader;
        private string _teleportSoundFilePath = "Assets/teleport_sound.mp3"; // Ensure the file path is correct

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);

            _waveOutEvent = new WaveOutEvent();
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

            var playerSpriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (playerSpriteSheet != null)
            {
                _player = new PlayerObject(playerSpriteSheet, 100, 100);
            }

            var teleporterSpriteSheet = SpriteSheet.LoadSpriteSheet("teleporter.json", "Assets", _renderer);
            if (teleporterSpriteSheet != null)
            {
                _teleporter1 = new TeleporterObject(teleporterSpriteSheet, 150, 150);
                _teleporter2 = new TeleporterObject(teleporterSpriteSheet, 300, 300);
            }

            // Load the teleport sound
            _audioFileReader = new AudioFileReader(_teleportSoundFilePath);

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        private void PlayTeleportSound()
        {
            if (_audioFileReader == null)
            {
                Console.WriteLine("Teleport sound not loaded");
                return;
            }

            if (_waveOutEvent.PlaybackState != PlaybackState.Playing)
            {
                _audioFileReader.Position = 0;
                _waveOutEvent.Init(_audioFileReader);
                _waveOutEvent.Play();
            }

        }

        public void CleanUp()
        {
            _waveOutEvent?.Stop();
            _waveOutEvent?.Dispose();
            _audioFileReader?.Dispose();
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
            bool isAttacking = _input.IsKeyAPressed();
            bool addBomb = _input.IsKeyBPressed();

            if (isAttacking)
            {
                var dir = up ? 1 : 0;
                dir += down ? 1 : 0;
                dir += left ? 1 : 0;
                dir += right ? 1 : 0;
                if (dir <= 1)
                {
                    _player.Attack(up, down, left, right);
                }
                else
                {
                    isAttacking = false;
                }
            }
            if (!isAttacking)
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }

            CheckTeleporterInteraction();

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, false);
            }

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if (gameObject is TemporaryGameObject)
                {
                    var tempObject = (TemporaryGameObject)gameObject;
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player.GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        private void CheckTeleporterInteraction()
        {
            var playerPosition = _player.Position;

            if (_teleporter1.IsAvailable() && IsNearTeleporter(playerPosition, _teleporter1.Position))
            {
                // Teleport to teleporter 2
                _player.Position = (_teleporter2.Position.X, _teleporter2.Position.Y);
                PlayTeleportSound();
                _teleporter1.Activate();
                _teleporter2.Activate();
            }
            else if (_teleporter2.IsAvailable() && IsNearTeleporter(playerPosition, _teleporter2.Position))
            {
                // Teleport to teleporter 1
                _player.Position = (_teleporter1.Position.X, _teleporter1.Position.Y);
                PlayTeleportSound();
                _teleporter1.Activate();
                _teleporter2.Activate();
            }
            else if ((!_teleporter1.IsAvailable() && !_teleporter2.IsAvailable()) && (!IsNearTeleporter(playerPosition, _teleporter1.Position) && !IsNearTeleporter(playerPosition, _teleporter2.Position)))
            {
                _teleporter1.Deactivate();
                _teleporter2.Deactivate();
            }
        }

        private bool IsNearTeleporter((int X, int Y) playerPosition, (int X, int Y) teleporterPosition)
        {
            var deltaX = Math.Abs(playerPosition.X - teleporterPosition.X);
            var deltaY = Math.Abs(playerPosition.Y - teleporterPosition.Y);
            return deltaX < 32 && deltaY < 32; // Adjust based on proximity threshold
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            // Render teleporters
            _teleporter1.Render(_renderer);
            _teleporter2.Render(_renderer);

            _renderer.PresentFrame();
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

        private void AddBomb(int x, int y, bool translateCoordinates = true)
        {
            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
    }
}
