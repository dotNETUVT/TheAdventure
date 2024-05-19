using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;

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

        private bool _displayPowerUpMessage = false;
        private int _displayPowerUpMessageId = 0;
        private DateTimeOffset _powerUpMessageStartTime;
        private PowerUpObject _currentPowerUp;

        private bool _displayCoins = false;
        private int _displayCoinsMessageId = 0;
        private DateTimeOffset _coinsMessageStartTime;

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

            WallObject.InitializeWalls(_renderer, _gameObjects);
            InitializePowerUps();
            InitializeObstacles();
        }

        private void InitializePowerUps()
        {
            var redChestSpriteSheet = SpriteSheet.LoadSpriteSheet("redChest.json", "Assets", _renderer);
            var blueChestSpriteSheet = SpriteSheet.LoadSpriteSheet("blueChest.json", "Assets", _renderer);
            var yellowChestSpriteSheet = SpriteSheet.LoadSpriteSheet("yellowChest.json", "Assets", _renderer);
            var finalChestSpriteSheet = SpriteSheet.LoadSpriteSheet("victoryChest.json", "Assets", _renderer);

            if (redChestSpriteSheet != null)
            {
                var redPowerUp = new PowerUpObject(redChestSpriteSheet, (370, 110), PowerUpType.Red);
                _gameObjects.Add(redPowerUp.Id, redPowerUp);
            }

            if (blueChestSpriteSheet != null)
            {
                var bluePowerUp = new PowerUpObject(blueChestSpriteSheet, (550, 160), PowerUpType.Blue);
                _gameObjects.Add(bluePowerUp.Id, bluePowerUp);
            }

            if (yellowChestSpriteSheet != null)
            {
                var yellowPowerUp = new PowerUpObject(yellowChestSpriteSheet, (650, 410), PowerUpType.Yellow);
                _gameObjects.Add(yellowPowerUp.Id, yellowPowerUp);
            }

            if (finalChestSpriteSheet != null)
            {
                var finalChest = new VictoryChestObject(finalChestSpriteSheet, (850, 420));
                _gameObjects.Add(finalChest.Id, finalChest);
            }
        }

        private void InitializeObstacles()
        {
            var redObstacleSpriteSheet = SpriteSheet.LoadSpriteSheet("redObstacle.json", "Assets", _renderer);
            var blueObstacleSpriteSheet = SpriteSheet.LoadSpriteSheet("blueObstacle.json", "Assets", _renderer);
            var yellowObstacleSpriteSheet = SpriteSheet.LoadSpriteSheet("yellowObstacle.json", "Assets", _renderer);

            var len = _gameObjects.Count;
            Console.WriteLine(len);

            if (redObstacleSpriteSheet != null)
            {
                var redObstacle = new ObstacleObject(redObstacleSpriteSheet, (360, 415), ObstacleType.Red);
                _gameObjects.Add(redObstacle.Id, redObstacle);
                if (_gameObjects.TryGetValue(len - 3, out var redPowerUp))
                {
                    ((PowerUpObject)redPowerUp).CorrespondingObstacle = redObstacle;
                }
            }

            if (blueObstacleSpriteSheet != null)
            {
                var blueObstacle = new ObstacleObject(blueObstacleSpriteSheet, (830, 575), ObstacleType.Blue);
                _gameObjects.Add(blueObstacle.Id, blueObstacle);
                if (_gameObjects.TryGetValue(len - 2, out var bluePowerUp))
                {
                    ((PowerUpObject)bluePowerUp).CorrespondingObstacle = blueObstacle;
                }
            }

            if (yellowObstacleSpriteSheet != null)
            {
                var yellowObstacle = new ObstacleObject(yellowObstacleSpriteSheet, (610, 575), ObstacleType.Yellow);
                _gameObjects.Add(yellowObstacle.Id, yellowObstacle);
                if (_gameObjects.TryGetValue(len - 1, out var yellowPowerUp))
                {
                    ((PowerUpObject)yellowPowerUp).CorrespondingObstacle = yellowObstacle;
                }
            }
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
            var itemsToRemove = GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList();

            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, false);
            }

            foreach (var gameObjectId in itemsToRemove)
            {
                if (_gameObjects.TryGetValue(gameObjectId, out var gameObject) && gameObject is TemporaryGameObject tempObject)
                {
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player.GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }

            CheckForObstacleCollisions(isAttacking);
            CheckForWallCollisions();
            CheckForBombProximity();
            UpdatePowerUpMessage();
            UpdateCoinsMessage();

            if (_player.HasYellowPowerUp && IsNear(_player.Position, (200, 200), 32))
            {
                AddBomb(200, 200, false);
                AddBomb(250, 200, false);
                AddBomb(300, 200, false);
            }
        }

        private void CheckForObstacleCollisions(bool isAttacking)
        {
            var obstacles = _gameObjects.Values.OfType<ObstacleObject>().ToList();

            foreach (var obstacle in obstacles)
            {
                var deltaX = Math.Abs(_player.Position.X - obstacle.Position.X);
                var deltaY = Math.Abs(_player.Position.Y - obstacle.Position.Y);

                if (deltaX < 32 && deltaY < 32)
                {
                    if (_player.CanDestroyObstacle(obstacle.ObstacleType) && isAttacking)
                    {
                        _currentPowerUp = _gameObjects.Values.OfType<PowerUpObject>()
                            .FirstOrDefault(pu => pu.CorrespondingObstacle == obstacle);

                        var victoryChest = _gameObjects.Values.OfType<VictoryChestObject>()
                            .FirstOrDefault(vc => vc.Position == obstacle.Position);

                        if (_currentPowerUp != null)
                        {
                            _displayPowerUpMessage = true;
                            _displayPowerUpMessageId = _currentPowerUp.Id + 1000;
                            _powerUpMessageStartTime = DateTimeOffset.Now;
                        }
                        else if (victoryChest != null)
                        {
                            Console.WriteLine($"Removing victory chest: {victoryChest.Id}");
                            _gameObjects.Remove(victoryChest.Id);
                            _displayCoins = true;
                            _coinsMessageStartTime = DateTimeOffset.Now;
                            CreateCoins();
                        }
                        else
                        {
                            Console.WriteLine($"Removing obstacle: {obstacle.Id}");
                            _gameObjects.Remove(obstacle.Id);
                        }
                    }
                    else
                    {
                        if (_player.Position.X > obstacle.Position.X && _player.Position.X < obstacle.Position.X + 32)
                        {
                            _player.Position = (obstacle.Position.X + 32, _player.Position.Y);
                        }
                        else if (_player.Position.X < obstacle.Position.X && _player.Position.X + 32 > obstacle.Position.X)
                        {
                            _player.Position = (obstacle.Position.X - 32, _player.Position.Y);
                        }

                        if (_player.Position.Y > obstacle.Position.Y && _player.Position.Y < obstacle.Position.Y + 32)
                        {
                            _player.Position = (_player.Position.X, obstacle.Position.Y + 32);
                        }
                        else if (_player.Position.Y < obstacle.Position.Y && _player.Position.Y + 32 > obstacle.Position.Y)
                        {
                            _player.Position = (_player.Position.X, obstacle.Position.Y - 32);
                        }
                    }
                }
            }
        }

        private void CheckForWallCollisions()
        {
            var walls = _gameObjects.Values.OfType<WallObject>().ToList();

            foreach (var wall in walls)
            {
                var wallWidth = wall.IsVertical ? wall.SpriteSheet.FrameHeight : wall.SpriteSheet.FrameWidth;
                var wallHeight = wall.IsVertical ? wall.SpriteSheet.FrameWidth : wall.SpriteSheet.FrameHeight;
                var deltaX = Math.Abs(_player.Position.X - wall.Position.X);
                var deltaY = Math.Abs(_player.Position.Y - wall.Position.Y);

                if (deltaX < wallWidth && deltaY < wallHeight)
                {
                    if (wall.IsVertical)
                    {
                        if (_player.Position.X > wall.Position.X && _player.Position.X < wall.Position.X + wallWidth)
                        {
                            _player.Position = (wall.Position.X + wallWidth, _player.Position.Y);
                        }
                        else if (_player.Position.X < wall.Position.X && _player.Position.X + wallWidth > wall.Position.X)
                        {
                            _player.Position = (wall.Position.X - wallWidth, _player.Position.Y);
                        }
                    }
                    else
                    {
                        if (_player.Position.Y > wall.Position.Y && _player.Position.Y < wall.Position.Y + wallHeight)
                        {
                            _player.Position = (_player.Position.X, wall.Position.Y + wallHeight);
                        }
                        else if (_player.Position.Y < wall.Position.Y && _player.Position.Y + wallHeight > wall.Position.Y)
                        {
                            _player.Position = (_player.Position.X, wall.Position.Y - wallHeight);
                        }
                    }
                }
            }
        }


        private void CheckForBombProximity()
        {
            var powerUps = _gameObjects.Values.OfType<PowerUpObject>().ToList();
            var bombs = _gameObjects.Values.OfType<TemporaryGameObject>().ToList();

            foreach (var bomb in bombs)
            {
                foreach (var powerUp in powerUps)
                {
                    var deltaX = Math.Abs(bomb.Position.X - powerUp.Position.X);
                    var deltaY = Math.Abs(bomb.Position.Y - powerUp.Position.Y);

                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player.CollectPowerUp(powerUp.PowerUpType);
                        _currentPowerUp = powerUp;
                        _displayPowerUpMessage = true;
                        _displayPowerUpMessageId = powerUp.Id + 1000000;
                        _powerUpMessageStartTime = DateTimeOffset.Now;
                    }
                }
            }
        }

        private void DisplayPowerUpMessage()
        {
            if (_displayPowerUpMessage)
            {
                var powerUpSpriteSheet = SpriteSheet.LoadSpriteSheet("powerup.json", "Assets", _renderer);
                if (powerUpSpriteSheet != null && (!_gameObjects.ContainsKey(_displayPowerUpMessageId)))
                {
                    var powerUpMessage = new TemporaryGameObject(powerUpSpriteSheet, 2.0, (_player.Position.X, _player.Position.Y - 50));
                    _gameObjects.Add(_displayPowerUpMessageId, powerUpMessage);
                }
            }
        }

        private void UpdatePowerUpMessage()
        {
            if (_displayPowerUpMessage && (DateTimeOffset.Now - _powerUpMessageStartTime).TotalSeconds > 2.0)
            {
                _gameObjects.Remove(_displayPowerUpMessageId);
                if (_currentPowerUp != null)
                {
                    Console.WriteLine($"Removing power-up: {_currentPowerUp.Id} and corresponding obstacle: {_currentPowerUp.CorrespondingObstacle?.Id}");
                    _gameObjects.Remove(_currentPowerUp.Id);
                    if (_currentPowerUp.CorrespondingObstacle != null)
                    {
                        _gameObjects.Remove(_currentPowerUp.CorrespondingObstacle.Id);
                    }
                }
                _displayPowerUpMessage = false;
            }
        }

        private void DisplayCoins()
        {
            if (_displayCoins)
            {
                var coinSpriteSheet = SpriteSheet.LoadSpriteSheet("coins.json", "Assets", _renderer);
                if (coinSpriteSheet != null && (!_gameObjects.ContainsKey(_displayCoinsMessageId)))
                {
                    CreateCoins();
                }
            }
        }

        private void CreateCoins()
        {
            var coinSpriteSheet = SpriteSheet.LoadSpriteSheet("coins.json", "Assets", _renderer);
            if (coinSpriteSheet == null) return;

            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(0, _currentLevel.Width * _currentLevel.TileWidth);
                int y = random.Next(0, _currentLevel.Height * _currentLevel.TileHeight);
                var coin = new TemporaryGameObject(coinSpriteSheet, 2.0, (x, y));
                _gameObjects.Add(coin.Id, coin);
            }
        }

        private void UpdateCoinsMessage()
        {
            if (_displayCoins && (DateTimeOffset.Now - _coinsMessageStartTime).TotalSeconds > 2.0)
            {
                _displayCoins = false;
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();
            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            DisplayPowerUpMessage();
            DisplayCoins();

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
                if (gameObject is WallObject wall)
                {
                    double angle = wall.IsVertical ? 90.0 : 0.0;
                    wall.Render(_renderer, angle);
                }
                else
                {
                    gameObject.Render(_renderer);
                }
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

        private bool IsNear((int X, int Y) position, (int X, int Y) target, int range)
        {
            var deltaX = Math.Abs(position.X - target.X);
            var deltaY = Math.Abs(position.Y - target.Y);
            return deltaX < range && deltaY < range;
        }
    }
}
