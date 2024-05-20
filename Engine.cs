using System.Reflection;
using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;

namespace TheAdventure
{
    public unsafe class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player1;
        private PlayerObject _player2;
        private GameRenderer _renderer;
        private Input _input;
        private ScriptEngine _scriptEngine;
        private HealthBar _healthBar1;
        private HealthBar _healthBar2;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
            _scriptEngine = new ScriptEngine();
            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
        }

        public void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public (int x, int y) GetPlayerPosition()
        {
            var pos = _player1.Position;
            return (pos.X, pos.Y);
        }

        public void InitializeWorld()
        {
            var executableLocation = new FileInfo(Assembly.GetExecutingAssembly().Location);
            _scriptEngine.LoadAll(Path.Combine(executableLocation.Directory.FullName, "Assets", "Scripts"));

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
                _player1 = new PlayerObject(spriteSheet, 100, 100);
                _player2 = new PlayerObject(spriteSheet, 200, 200);
            }

            _healthBar1 = new HealthBar(_renderer._sdl, _renderer._renderer, _player1.GetMaxHealth());
            _healthBar2 = new HealthBar(_renderer._sdl, _renderer._renderer, _player2.GetMaxHealth());

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up1 = _input.IsUpPressed();
            bool down1 = _input.IsDownPressed();
            bool left1 = _input.IsLeftPressed();
            bool right1 = _input.IsRightPressed();
            bool isAttacking1 = _input.IsKeyAPressed();
            bool addBomb1 = _input.IsKeyBPressed();

            bool up2 = _input.IsWPressed();
            bool down2 = _input.IsSPressed();
            bool left2 = _input.IsAPressed();
            bool right2 = _input.IsDPressed();
            bool isAttacking2 = _input.IsKeyQPressed();
            bool addBomb2 = _input.IsKeyEPressed();

            _scriptEngine.ExecuteAll(this);

            ProcessPlayer(_player1, _healthBar1, up1, down1, left1, right1, isAttacking1, addBomb1, secsSinceLastFrame);
            ProcessPlayer(_player2, _healthBar2, up2, down2, left2, right2, isAttacking2, addBomb2, secsSinceLastFrame);

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if (gameObject is TemporaryGameObject tempObject)
                {
                    if (IsPlayerHitByTempObject(_player1, tempObject, _healthBar1) || IsPlayerHitByTempObject(_player2, tempObject, _healthBar2))
                    {
                        // Handle collision logic if needed
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        private void ProcessPlayer(PlayerObject player, HealthBar healthBar, bool up, bool down, bool left, bool right, bool isAttacking, bool addBomb, double secsSinceLastFrame)
        {
            if (isAttacking)
            {
                var dir = up ? 1 : 0;
                dir += down ? 1 : 0;
                dir += left ? 1 : 0;
                dir += right ? 1 : 0;
                if (dir <= 1)
                {
                    player.Attack(up, down, left, right);
                }
                else
                {
                    isAttacking = false;
                }
            }
            if (!isAttacking)
            {
                player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }
            if (addBomb)
            {
                AddBomb(player.Position.X, player.Position.Y, false);
            }
        }

        private bool IsPlayerHitByTempObject(PlayerObject player, TemporaryGameObject tempObject, HealthBar healthBar)
        {
            var deltaX = Math.Abs(player.Position.X - tempObject.Position.X);
            var deltaY = Math.Abs(player.Position.Y - tempObject.Position.Y);
            if (deltaX < 32 && deltaY < 32)
            {
                player.DecreaseHealth(20); // Reduce health by 20%
                healthBar.DecreaseHealth(20);
                return true;
            }
            return false;
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player1.Position.X, _player1.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            _healthBar1.Render(10, 10, 200, 20);
            _healthBar2.Render(10, 40, 200, 20);

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

            _player1.Render(_renderer);
            _player2.Render(_renderer);
        }

        public void AddBomb(int x, int y, bool translateCoordinates = true)
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
