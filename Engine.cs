using System.Reflection;
using System.Text.Json;
using Silk.NET.Input;
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
        private ScriptEngine _scriptEngine;
        private bool _bombAddedRecently = false;
        private bool _projectileAddedRecently = false;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        private int _knifeTextureId;
        private int _knifeWidth;
        private int _knifeHeight;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
            _scriptEngine = new ScriptEngine();
            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y, true);
            _knifeTextureId = _renderer.LoadTexture("Assets/Knife.png", out var size);
            _knifeWidth = size.Width;
            _knifeHeight = size.Height;
        }

        public void WriteToConsole(string message)
        {
            Console.WriteLine(message);
        }

        public (int x, int y) GetPlayerPosition()
        {
            var pos = _player.Position;
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
                _player = new PlayerObject(spriteSheet, 100, 100, this);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight));
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
            bool launchProjectile = _input.IsKeyCPressed(); // Assign a key for launching projectiles
            bool addBomb = _input.IsKeyBPressed();

            _scriptEngine.ExecuteAll(this);

            UpdateProjectiles(secsSinceLastFrame);

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

            if (launchProjectile && !_projectileAddedRecently)
            {
                _projectileAddedRecently = true;
                _player.LaunchProjectile(_knifeTextureId, _knifeWidth, _knifeHeight);
            }

            if (!isAttacking)
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            if (addBomb && !_bombAddedRecently)
            {
                _bombAddedRecently = true;
                AddBomb(0, 0);
            }

            if (!_input.IsKeyBPressed())
            {
                _bombAddedRecently = false;
            }

            if (!_input.IsKeyCPressed())
            {
                _projectileAddedRecently = false;
            }

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if (gameObject is TemporaryGameObject tempObject)
                {
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player.GameOver();
                    }
                }
                if (gameObject is ProjectileObject projectile)
                {
                    // Add debug output to check if the Update method is called for projectiles
                    projectile.Update(secsSinceLastFrame);
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

        private void AddBomb(int offsetX, int offsetY, bool isMousePlacement = false)
        {
            int bombX, bombY;

            if (isMousePlacement)
            {
                bombX = offsetX;
                bombY = offsetY;
            }
            else
            {
                bombX = _player.Position.X + offsetX;
                bombY = _player.Position.Y + offsetY;

                var playerDirection = _player.State.Direction;
                switch (playerDirection)
                {
                    case PlayerObject.PlayerStateDirection.Up:
                        bombY -= 40;
                        break;
                    case PlayerObject.PlayerStateDirection.Down:
                        bombY += 40;
                        break;
                    case PlayerObject.PlayerStateDirection.Left:
                        bombX -= 40;
                        break;
                    case PlayerObject.PlayerStateDirection.Right:
                        bombX += 40;
                        break;
                    default:
                        break;
                }
            }

            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (bombX, bombY));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }

        public void UpdateProjectiles(double deltaTime)
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is ProjectileObject projectile)
                {
                    projectile.Update(deltaTime);
                }
            }
        }

        public void AddProjectile(ProjectileObject projectile)
        {
            _gameObjects.Add(projectile.Id, projectile);
        }


    }
}

