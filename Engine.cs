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
        private readonly Dictionary<string, SpriteSheet> _loadedStarSpriteSheet = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastStarTime = DateTimeOffset.Now;
        private readonly TimeSpan _starInterval = TimeSpan.FromSeconds(6);
        private Dictionary<char, int> _numberWidths = new() 
        {
            { '0', 14 },
            { '1', 8 },
            { '2', 14 },
            { '3', 14 },
            { '4', 14 },
            { '5', 14 },
            { '6', 14 },
            { '7', 14 },
            { '8', 14 },
            { '9', 14 }
        };
        
        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);

            _renderer.LoadStarTexture("Assets/star.png");
            _renderer.LoadNumberTexture("Assets/numbers.png");
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
            
            var starSpriteSheet = SpriteSheet.LoadSpriteSheet("star.json", "Assets", _renderer);
            if (starSpriteSheet != null)
                _loadedStarSpriteSheet["star"] = starSpriteSheet;
            else
                Console.WriteLine("Failed to load star sprite sheet.");
            
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
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();
            bool isAttacking = _input.IsKeyAPressed();
            bool addBomb = _input.IsKeyBPressed();

            if(isAttacking)
            {
                var dir = up ? 1: 0;
                dir += down? 1 : 0;
                dir += left? 1: 0;
                dir += right ? 1 : 0;
                if(dir <= 1){
                    _player.Attack(up, down, left, right);
                }
                else{
                    isAttacking = false;
                }
            }
            if(!isAttacking)
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
                if(gameObject is TemporaryGameObject){
                    var tempObject = (TemporaryGameObject)gameObject;
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if(deltaX < 32 && deltaY < 32){
                        _player.GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
            
            var stars = _gameObjects.Values.OfType<StarObject>().ToList();
            foreach (var star in stars)
            {
                if (IsIntersection(_player.Position, star.Position, 15))  
                {
                    _player.GetStar();
                    _gameObjects.Remove(star.Id);
                }
            }
            
            string starsTotal = _player?.StarsTotal.ToString() ?? "0";
            for (int i = 0; i < starsTotal.Length; i++)
                _renderer.RenderNumber(starsTotal[i], 50 + i * 20, 10);
            
            if ((currentTime - _lastStarTime) > _starInterval)
            {
                SpawnStar();
                _lastStarTime = currentTime;
            }
            
            var unavailableStars = _gameObjects.Values.OfType<StarObject>().Where(m => m.IsUnavailable).ToList();
            foreach (var star in unavailableStars)
                _gameObjects.Remove(star.Id);
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();
            
            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();
            
            _renderer.RenderStar(10, 10, scale: 0.5f);  
            
            string starsTotal = _player?.StarsTotal.ToString() ?? "0";
            int xPos = 40;
            for (int i = 0; i < starsTotal.Length; i++)
            {
                char number = starsTotal[i];
                _renderer.RenderNumber(number, xPos, 13, scale: 1.2f);
                xPos += (int)(_numberWidths[number] * 0.5f) + 2;
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
                if (gameObject is StarObject star)
                    star.Render(_renderer, scale: 0.5f);
                else
                    gameObject.Render(_renderer);
            }

            _player.Render(_renderer);
        }

        private void AddBomb(int x, int y, bool translateCoordinates = true)
        {

            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);
            
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if(spriteSheet != null){
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
        
        private void AddStar(int x, int y, double ttl = 10.0)  
        {
            if (_loadedStarSpriteSheet.TryGetValue("star", out var spriteSheet))
            {
                StarObject star = new(spriteSheet, (x, y), ttl);
                _gameObjects.Add(star.Id, star);
            }
            else
                Console.WriteLine("Failed to find star sprite sheet.");
        }


        private void SpawnStar()
        {
            var random = new Random();
            int x = random.Next(10, _currentLevel.Width * _currentLevel.TileWidth - 10);
            int y = random.Next(24, _currentLevel.Height * _currentLevel.TileHeight - 24);
            AddStar(x, y);
        }
        
        private bool IsIntersection((int X, int Y) pos1, (int X, int Y) pos2, int size)
        {
            return Math.Abs(pos1.X - pos2.X) < size && Math.Abs(pos1.Y - pos2.Y) < size;
        }
    }
}