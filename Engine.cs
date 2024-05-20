using System.Reflection;
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
        private ScriptEngine _scriptEngine;

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

        public PlayerObject GetPlayer()
        {
            return _player;
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
                _player = new PlayerObject(spriteSheet, 100, 100);
            }

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        private void UpdateBombState(TemporaryGameObject bomb)
        {
            // If the bomb's TTL (Time to Live) has expired, mark it as exploding
            if (bomb.IsExpired)
            {
                bomb.IsExploding = true;
                _gameObjects.Remove(bomb.Id); // Remove the bomb from the game objects collection
            }
        }

        public void ProcessFrame()
        {
            _scriptEngine.ExecuteAll(this);

            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();
            bool isAttacking = _input.IsKeyAPressed();
            bool addBomb = _input.IsKeyBPressed();
            bool reset = _input.IsKeyRPressed();

            if (_player.IsDead())
            {
                if (reset)
                {
                    ResetGame();
                }
            }
            else
            {
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

                var itemsToRemove = new List<int>();
                var playerAttackedBomb = false;

                foreach (var gameObjectId in _gameObjects.Keys.ToList())
                {
                    var gameObject = _gameObjects[gameObjectId];
                    if (gameObject is TemporaryGameObject tempObject && tempObject.Type == GameObjectType.Bomb)
                    {
                        UpdateBombState(tempObject); // Update the bomb's state

                        // Check player's distance to the bomb
                        var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                        var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                        // If the player is attacking near the bomb, remove the bomb immediately
                        if (isAttacking && distance < 32) // Adjust the proximity threshold as needed
                        {
                            itemsToRemove.Add(gameObjectId); // Remove the bomb immediately
                        }
                        // If the bomb is exploding and the player is within range
                        else if (tempObject.IsExploding && distance < 100) // Replace 32 with the desired interaction radius
                        {
                            if (isAttacking)
                            {
                                itemsToRemove.Add(gameObjectId); // Remove the bomb if the player attacks it
                            }
                            else
                            {
                                _player.GameOver(); // Otherwise, trigger player's game over
                            }
                        }
                    }
                }

                if (addBomb)
                {
                    AddBomb(_player.Position.X, _player.Position.Y, false);
                }

                // Remove expired objects
                foreach (var gameObjectId in itemsToRemove)
                {
                    _gameObjects.Remove(gameObjectId);
                }

                // Ensure player doesn't die if they successfully attacked a bomb
                if (playerAttackedBomb && _player.IsDead())
                {
                    _player.Reset(); // Or handle player state reset if necessary
                }
            }
        }

        public void ResetGame()
        {
            _player.Reset();
            _gameObjects.Clear();
            _scriptEngine.ClearScripts();
            InitializeWorld();
        }

        public void AddBomb(int x, int y, bool translateCoordinates = true)
        {
            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y), GameObjectType.Bomb);
                bomb.IsExploding = false; // Set initially to false
                _gameObjects.Add(bomb.Id, bomb);
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

    }
}