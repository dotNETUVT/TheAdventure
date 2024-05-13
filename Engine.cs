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
        private DogObject _dog;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastRandomBombTime = DateTimeOffset.Now;
        private Random _random = new Random();

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
            SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, (24, 42));
            
            spriteSheet.Animations["WalkRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (4, 0),
                EndFrame = (4, 5),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet.Animations["WalkLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (4, 0),
                EndFrame = (4, 5),
                DurationMs = 1000,
                Loop = true,
                Flip = RendererFlip.Horizontal
            };

            spriteSheet.Animations["WalkUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (5, 0),
                EndFrame = (5, 5),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet.Animations["WalkDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 5),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet.Animations["IdleLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 5),
                DurationMs = 1000,
                Loop = true,
                Flip = RendererFlip.Horizontal
            };

            spriteSheet.Animations["IdleRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 5),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet.Animations["IdleUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 5),
                DurationMs = 1000,
                Loop = true
            };
            
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 5),
                DurationMs = 1000,
                Loop = true
            };
            
            spriteSheet.Animations["Stay"] = new SpriteSheet.Animation()
            {
                StartFrame = (9, 0),
                EndFrame = (9, 3),
                DurationMs = 2000,
                Loop = true
            };
            
            _player = new PlayerObject(spriteSheet, 100, 100);
            
            SpriteSheet spriteSheet2 = new(_renderer, Path.Combine("Assets", "dog.png"), 10, 6, 32, 32, (24, 52));

             spriteSheet2.Animations["WalkRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 3),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet2.Animations["WalkLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 3),
                DurationMs = 1000,
                Loop = true,
            };

            spriteSheet2.Animations["WalkUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 3),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet2.Animations["WalkDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 3),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet2.Animations["IdleLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 3),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet2.Animations["IdleRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 3),
                DurationMs = 1000,
                Loop = true
            };

            spriteSheet2.Animations["IdleUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 3),
                DurationMs = 1000,
                Loop = true
            };
            
            spriteSheet2.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 3),
                DurationMs = 1000,
                Loop = true
            };
            
            spriteSheet2.Animations["Stay"] = new SpriteSheet.Animation()
            {
                StartFrame = (7, 0),
                EndFrame = (7, 1),
                DurationMs = 2000,
                Loop = true
            };
            
            _dog = new DogObject(spriteSheet2, 85, 120);

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

            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);
            
            _dog.UpdateDogPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());
            
            CheckForBombCollisions();

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }

            if ((currentTime - _lastRandomBombTime).TotalSeconds > _random.Next(1, 2))
            {
                AddRandomBomb();
                _lastRandomBombTime = currentTime;
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
            Random random = new Random();

            for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
            {
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j)
                    {
                        var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                        var tileVariations = _loadedTileSets
                            .SelectMany(ts => ts.Value.Tiles.Where(t => t.Id == cTileId)).ToList();

                        var cTile = tileVariations.Count > 1
                            ? tileVariations[random.Next(tileVariations.Count)]
                            : GetTile(cTileId);
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
            _dog.Render(_renderer);
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
            TemporaryGameObject bomb = new(spriteSheet, 10, (translated.X, translated.Y));
            _gameObjects.Add(bomb.Id, bomb);
        }

        private void AddRandomBomb()
        {
            int randomX = _random.Next(0, _currentLevel.Width);
            int randomY = _random.Next(0, _currentLevel.Height);
            AddBomb(randomX * _currentLevel.TileWidth, randomY * _currentLevel.TileHeight);
        }
        
        private void CheckForBombCollisions()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject bomb)
                {
                    if (IsPlayerCollidingWithBomb(_player.Position, bomb.Position))
                    {
                        TriggerBombExplosion(bomb);
                    }
                }
            }
        }

        private bool IsPlayerCollidingWithBomb((int X, int Y) playerPosition, (int X, int Y) bombPosition)
        {
            const int collisionThreshold = 48; 
            return Math.Abs(playerPosition.X - bombPosition.X) < collisionThreshold &&
                   Math.Abs(playerPosition.Y - bombPosition.Y) < collisionThreshold;
        }

        private void TriggerBombExplosion(TemporaryGameObject bomb)
        {
            bomb.SpriteSheet.ActivateAnimation("Explode");
        }
    }
}