using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;
using System;

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

        // Speed boost variables
        private bool isSpeedBoostActive = false;
        private float speedBoostDuration = 2.0f; // Speed boost duration in seconds
        private float speedBoostTimer = 0.0f;
        private float normalSpeed = 2.0f;
        private float boostedSpeed = 4.0f; // Significantly higher speed for visibility
        private float playerSpeed;

        // Timer for automatic speed boost
        private float autoBoostInterval = 5.0f; // Interval for automatic speed boost in seconds
        private float autoBoostTimer = 0.0f;

        // Game over variables
        private bool isGameOver = false;
        private DateTimeOffset gameOverStartTime;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
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

            playerSpeed = normalSpeed; // Initialize player speed
            autoBoostTimer = autoBoostInterval; // Initialize the auto boost timer
        }

        public void ProcessFrame()
        {
            if (isGameOver)
            {
                // Check if 5 seconds have passed since the game over
                if ((DateTimeOffset.Now - gameOverStartTime).TotalSeconds >= 5)
                {
                    Environment.Exit(0); // Close the game
                }
                return; // Skip the rest of the frame processing
            }

            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();
            bool isAttacking = _input.IsKeyAPressed();
            bool addBomb = _input.IsKeyBPressed();

            // Update speed boost timer
            if (isSpeedBoostActive)
            {
                speedBoostTimer -= (float)secsSinceLastFrame;
                if (speedBoostTimer <= 0)
                {
                    isSpeedBoostActive = false;
                    playerSpeed = normalSpeed;
                }
            }

            // Update automatic boost timer
            autoBoostTimer -= (float)secsSinceLastFrame;
            if (autoBoostTimer <= 0)
            {
                ActivateSpeedBoost();
                autoBoostTimer = autoBoostInterval;
            }

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
                    secsSinceLastFrame * playerSpeed);
            }
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
                        GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            if (isGameOver)
            {
                _renderer.RenderGameOverMessage();
            }

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

        private void ActivateSpeedBoost()
        {
            Console.WriteLine("Speed boost activated"); // Debug statement
            if (!isSpeedBoostActive)
            {
                isSpeedBoostActive = true;
                speedBoostTimer = speedBoostDuration;
                playerSpeed = boostedSpeed;
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            gameOverStartTime = DateTimeOffset.Now;
            Console.WriteLine("Game Over"); // Debug statement
        }
    }
}
