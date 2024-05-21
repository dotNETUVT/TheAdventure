using System.Reflection;
using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;
using TheAdventure.Models.Data;
using System.Timers;
using static TheAdventure.GameRenderer;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();
        
        private Sdl _sdl; // Add this line to define _sdl

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;
        private ScriptEngine _scriptEngine;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        private DateTimeOffset _gameOverTime;
        private bool _isGameOverTriggered = false;

        public Engine(GameRenderer renderer, Input input, Sdl sdl) // Add Sdl parameter to the constructor
        {
            _renderer = renderer;
            _input = input;
            _sdl = sdl; // Initialize _sdl
            _scriptEngine = new ScriptEngine();
            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
            _renderer.LoadGameOverTexture(Path.Combine("Assets", "gameover.png"));
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
                _player = new PlayerObject(spriteSheet, 100, 100, 150); // Initialize with 150 HP
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            if (_isGameOverTriggered)
            {
                var currentTime2 = DateTimeOffset.Now;
                if ((currentTime2 - _gameOverTime).TotalSeconds > 5)
                {
                    CloseGame();
                    return;
                }
            }

            if (_input.IsEscapePressed())
            {
                CloseGame();
                return;
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

            bool isMoving = up || down || left || right;

            _scriptEngine.ExecuteAll(this);

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

            if (isMoving && _player.HasEnergy)
            {
                _player.DepleteEnergy(secsSinceLastFrame);
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }
            else if (!isMoving)
            {
                _player.RechargeEnergy(secsSinceLastFrame);
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
                if (gameObject is TemporaryGameObject tempObject)
                {
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player.TakeDamage();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            if (_renderer.IsGameOver)
            {
                _renderer.RenderGameOverScreen();
                _renderer.PresentFrame();
                return;
            }

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);
            RenderTerrain();
            RenderAllObjects();

            RenderHPBar();
            RenderEnergyBar();

            _renderer.PresentFrame();
        }

        private void CheckPlayerGameOver()
        {
            if (_player.State.State == PlayerObject.PlayerState.GameOver && !_isGameOverTriggered)
            {
                _gameOverTime = DateTimeOffset.Now;
                _isGameOverTriggered = true;
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

        private void RenderHPBar()
        {
            var hpBarWidth = 50;
            var hpBarHeight = 10;
            var hpBarX = 10;
            var hpBarY = 10;

            _renderer.SetDrawColor(128, 128, 128, 255);
            _renderer.RenderFillRectangle(hpBarX, hpBarY, hpBarWidth, hpBarHeight);

            var currentHpWidth = (int)((_player.HP / 150.0) * hpBarWidth); // Adjust for new HP value
            _renderer.SetDrawColor(255, 0, 0, 255); // Change color to red
            _renderer.RenderFillRectangle(hpBarX, hpBarY, currentHpWidth, hpBarHeight);
        }

        private void RenderEnergyBar()
        {
            var energyBarWidth = 50;
            var energyBarHeight = 10;
            var energyBarX = 10;
            var energyBarY = 25;

            _renderer.SetDrawColor(128, 128, 128, 255);
            _renderer.RenderFillRectangle(energyBarX, energyBarY, energyBarWidth, energyBarHeight);

            var currentEnergyWidth = (int)((_player.Energy / 4.0) * energyBarWidth);
            _renderer.SetDrawColor(0, 0, 255, 255);
            _renderer.RenderFillRectangle(energyBarX, energyBarY, currentEnergyWidth, energyBarHeight);
        }

        public void AddBomb(int x, int y, bool translateCoordinates = true)
        {
            var translated = translateCoordinates
                ? _renderer.TranslateFromScreenToWorldCoordinates(x, y)
                : new Vector2D<int>(x, y);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);

                // Check if player is near the bomb and trigger game over if needed
                CheckPlayerGameOver();
            }
        }

        private void CloseGame()
        {
            _sdl.Quit(); // Ensure that SDL is properly cleaned up
            Environment.Exit(0);
            
        }
    }
}
