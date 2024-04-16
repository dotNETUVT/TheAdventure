using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;

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
        
        private Dictionary<int,Coin> _coins = new Dictionary<int, Coin>();
        private Random _random = new Random();
        
        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
        }

      
        public void GenerateCoins(int numberOfCoins)
        {
            // Resolve the asset path correctly.
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Models\Coin.png");
            assetPath = Path.GetFullPath(assetPath);

            // Check if the asset file exists to avoid runtime errors.
            if (!File.Exists(assetPath))
            {
                Console.WriteLine($"Asset file not found: {assetPath}");
                return;
            }
            else
            {
                Console.WriteLine($"Asset file found: {assetPath}");
            }

            
            SpriteSheet coinSpriteSheet = new(_renderer, assetPath, 1, 1, 32, 32, (16, 16));
           
            
            if (_currentLevel == null || _currentLevel.Width <= 0 || _currentLevel.TileWidth <= 0 || _currentLevel.Height <= 0 || _currentLevel.TileHeight <= 0)
            {
                Console.WriteLine("Invalid level dimensions, unable to place coins.");
                return;
            }

            
            for (int i = 0; i < numberOfCoins; i++)
            {
                int x = _random.Next(10, _currentLevel.Width * _currentLevel.TileWidth);
                int y = _random.Next(10, _currentLevel.Height * _currentLevel.TileHeight);
                coinSpriteSheet.Animations["Idle"] = new SpriteSheet.Animation {
                    StartFrame = (0, 0),
                    EndFrame = (0, 0),
                    DurationMs = 1000,
                    Loop = true
                };

                Coin coin = new Coin(coinSpriteSheet, x, y);
                _coins.Add(coin.Id, coin); 
            }
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
            SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, (24, 42));
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 5),
                DurationMs = 600,
                Loop = true
            };
            spriteSheet.Animations["WalkingDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 5),
                DurationMs = 600,
                Loop = true
            };
            spriteSheet.Animations["WalkingUp"] = new SpriteSheet.Animation {
                StartFrame = (5, 0), 
                EndFrame = (5, 5),   
                DurationMs = 600,
                Loop = true
            };
        
            spriteSheet.Animations["WalkingLeft"] = new SpriteSheet.Animation {
                StartFrame = (1, 0), 
                EndFrame = (1, 3),   
                DurationMs = 600,
                Loop = true,
                Flip = RendererFlip.Horizontal
            };
            spriteSheet.Animations["WalkingRight"] = new SpriteSheet.Animation {
                StartFrame = (4, 0), 
                EndFrame = (4, 3),   
                DurationMs = 600,
                Loop = true
            };
          
            _player = new PlayerObject(spriteSheet, 100, 100);

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
           GenerateCoins(_random.Next(5, 15));
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
            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            var coinsToRemove = new List<int>();
            foreach (var coin in _coins.Values)
            {
                
                var playerBox = new Rectangle<float>(_player.Position.X, _player.Position.Y, _player.SpriteSheet.FrameWidth, _player.SpriteSheet.FrameHeight);
                var coinBox = new Rectangle<float>(coin.Position.X, coin.Position.Y, coin.SpriteSheet.FrameWidth, coin.SpriteSheet.FrameHeight);
            
                
                if (playerBox.Contains(coinBox)||coinBox.Contains(playerBox))
                {
                    coinsToRemove.Add(coin.Id);
                }
               
            }
            foreach (var coin in coinsToRemove)
            {
                _coins.Remove(coin);
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
            foreach (var coin in _coins.Values)
            {
                coin.Render(_renderer);
            }
            
        }

        private void AddBomb(int x, int y)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            SpriteSheet spriteSheet = new(_renderer, "BombExploding.png", 1, 13, 32, 64, (16, 48));
            spriteSheet.Animations["Explode"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 12),
                DurationMs = 2000,
                Loop = false
            };
            spriteSheet.ActivateAnimation("Explode");
            TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
            _gameObjects.Add(bomb.Id, bomb);
        }
    }
}