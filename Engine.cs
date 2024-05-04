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
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        public bool IsGameOver { get; private set; }

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) =>
            {
                
                if (IsGameOver && IsWithinPlayImage(coords.x, coords.y))
                {
                    RestartGame();
                }
                else
                {
                    AddBomb(coords.x, coords.y);
                }
            };
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
            if(spriteSheet != null){
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            if (IsGameOver)
                return;

            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();

            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame, GetAllTemporaryGameObjects());

            // Check collision between player and bombs
            foreach (var gameObject in GetAllTemporaryGameObjects())
            {
                if (gameObject is TemporaryGameObject bomb)
                {
                    if (RectanglesIntersect(_player.GetBoundingBox(), bomb.GetBoundingBox()))
                    {
                        // Check proximity
                        double distance = Math.Sqrt(Math.Pow(_player.Position.X - bomb.Position.X, 2) + Math.Pow(_player.Position.Y - bomb.Position.Y, 2));
                        if (distance < 50 && bomb.IsExpired) 
                        {
                            GameOver();
                            return;
                        }
                    }
                }
            }

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        // Method to check intersection between two rectangles
        private bool RectanglesIntersect(System.Drawing.Rectangle rect1, System.Drawing.Rectangle rect2)
        {
            return rect1.Left < rect2.Right && rect1.Right > rect2.Left &&
                   rect1.Top < rect2.Bottom && rect1.Bottom > rect2.Top;
        }

        // Handling game over scenario
        private void GameOver()
        {
            IsGameOver = true;
        }

        // Position for play again button
        private bool IsWithinPlayImage(int x, int y)
        {
            return x >= 385 && x <= 435 &&
                   y >= 290 && y <= 340;
        }

        // Handling restart game scenario
        private void RestartGame()
        {
            IsGameOver = false;
            InitializeWorld();

            ProcessFrame();
            RenderFrame();
        }

        public void RenderFrame()
        {
            if (!IsGameOver)
            {
                _renderer.SetDrawColor(0, 0, 0, 255);
                _renderer.ClearScreen();

                _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);
               
                RenderTerrain();
                RenderAllObjects();
            }
            else
            {
                // Render background color for game over screen
                _renderer.SetDrawColor(0, 0, 0, 255);
                _renderer.ClearScreen();

                // Render game over image
                var spriteSheet = new SpriteSheet(_renderer, "Assets/game_over.png", 1, 1, 200, 161, new FrameOffset() { OffsetX = 48, OffsetY = 48 });
                var gameOverObject = new RenderableGameObject(spriteSheet, (_renderer._camera.X - 45, _renderer._camera.Y - 75));
                gameOverObject.Render(_renderer);

                // Render replay image
                var spriteSheet1 = new SpriteSheet(_renderer, "Assets/play.png", 1, 1, 80, 80, new FrameOffset() { OffsetX = 50, OffsetY = 50 });
                var gameOverObject1 = new RenderableGameObject(spriteSheet1, (_renderer._camera.X + 5, _renderer._camera.Y + 75));
                gameOverObject1.Render(_renderer);
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
                    // Change for multiple layer rendering
                    if (tile.InternalTextureId == id)
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
            /*SpriteSheet spriteSheet = new(_renderer, "BombExploding.png", 1, 13, 32, 64, (16, 48));
            spriteSheet.Animations["Explode"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 12),
                DurationMs = 2000,
                Loop = false
            };*/
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if(spriteSheet != null){
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
    }
}