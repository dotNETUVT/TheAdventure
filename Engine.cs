using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using System.Timers;
using System.Media;
using TheAdventure.Models;
using TheAdventure.Models.Data;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private System.Timers.Timer _debugTimer;

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;
        private int score = 0;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

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

            SoundPlayer music = new SoundPlayer(@"Assets\music.wav");
            music.Play();

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
            /*SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, new FrameOffset() { OffsetX = 24, OffsetY = 42 });
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = new FramePosition(),//(0, 0),
                EndFrame = new FramePosition() { Row = 0, Col = 5 },
                DurationMs = 1000,
                Loop = true
            };
            */
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));

            var boulderSpriteSheet = SpriteSheet.LoadSpriteSheet("boulder.json", "Assets", _renderer);
            if (boulderSpriteSheet != null)
            {
                int mazeWidth = _currentLevel.Width;
                int mazeHeight = _currentLevel.Height;
                MazeGenerator mazeGenerator = new MazeGenerator(mazeWidth, mazeHeight);
                bool[,] maze = mazeGenerator.GenerateMaze();

                int playerStartX = _player.Position.X / _currentLevel.TileWidth;
                int playerStartY = _player.Position.Y / _currentLevel.TileHeight;
                int safeZoneRadius = 2;

                for (int x = 0; x < mazeWidth; x++)
                {
                    for (int y = 0; y < mazeHeight; y++)
                    {
                        bool isInSafeZone = Math.Abs(x - playerStartX) <= safeZoneRadius &&
                                            Math.Abs(y - playerStartY) <= safeZoneRadius;

                        if (!maze[x, y] && !isInSafeZone)
                        {
                            int worldX = x * _currentLevel.TileWidth;
                            int worldY = y * _currentLevel.TileHeight;
                            var boulder = new Boulder(boulderSpriteSheet, worldX, worldY);
                            _gameObjects.Add(boulder.Id, boulder);
                        }
                    }
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
            bool Space = _input.IsKeySpacePressed();

            UpdateMovementFlags();

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

        private void UpdateMovementFlags()
        {
            _player.CanMoveUp = true;
            _player.CanMoveDown = true;
            _player.CanMoveLeft = true;
            _player.CanMoveRight = true;

            int playerX = _player.Position.X;
            int playerY = _player.Position.Y;
            int playerWidth = _player.SpriteSheet.FrameWidth;
            int playerHeight = _player.SpriteSheet.FrameHeight;

            foreach (var gameObject in _gameObjects.Values.ToList())
            {
                if (gameObject is Boulder boulder)
                {
                    int boulderX = boulder.Position.X;
                    int boulderY = boulder.Position.Y;
                    int boulderWidth = boulder.SpriteSheet.FrameWidth;
                    int boulderHeight = boulder.SpriteSheet.FrameHeight;

                    int minX = _player.Position.X - playerWidth / 3;
                    int minY = _player.Position.Y - playerHeight / 3;
                    int maxX = _player.Position.X + playerWidth / 3;
                    int maxY = _player.Position.Y + playerHeight / 3;

                    if (boulderX >= minX && boulderX < maxX &&
                        boulderY >= minY && boulderY < maxY)
                    {
                        if (boulderX + boulderWidth / 2 < _player.Position.X)
                        {
                            _player.CanMoveLeft = false;
                        }
                        else if (boulderX + boulderWidth / 2 > _player.Position.X)
                        {
                            _player.CanMoveRight = false;
                        }

                        if (boulderY + boulderHeight / 2 < _player.Position.Y)
                        {
                            _player.CanMoveUp = false;
                        }
                        else if (boulderY + boulderHeight / 2 > _player.Position.Y)
                        {
                            _player.CanMoveDown = false;
                        }

                        if (_input.IsKeySpacePressed())
                        {
                            _gameObjects.Remove(boulder.Id);
                            score++;
                            Console.WriteLine($"Score: {score}");
                        }
                    }
                }
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