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
        private CatObject _cat;
        private GameRenderer _renderer;
        private Input _input;

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
                StartFrame = (0, 0),
                EndFrame = (0, 5),
                DurationMs = 1000,
                Loop = true
            };
            _player = new PlayerObject(spriteSheet, 100, 100);

            // Create the sprites animations and instanciate the 3 posible companions
            // to interact with the player during the adventure
            InitializeCompanions();

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void InitializeCompanions()
        {

            SpriteSheet dogSheet = new(_renderer, Path.Combine("Assets", "dog.png"), 9, 4, 32, 32, (24, 16));
            dogSheet.Animations["Idle"] = new SpriteSheet.Animation()
            {
                StartFrame = (6, 0),
                EndFrame = (6, 2),
                DurationMs = 1000,
                Loop = true
            };

            dogSheet.Animations["LeftMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 3),
                DurationMs = 1000,
                Loop = true
            };

            dogSheet.Animations["RightMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (8, 0),
                EndFrame = (8, 2),
                DurationMs = 1000,
                Loop = true
            };

            dogSheet.Animations["UpMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 3),
                DurationMs = 1000,
                Loop = true
            };

            dogSheet.Animations["DownMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 3),
                DurationMs = 1000,
                Loop = true
            };

            SpriteSheet catSheet = new(_renderer, Path.Combine("Assets", "cat.png"), 8, 4, 32, 32, (24, 16));
            catSheet.Animations["Idle"] = new SpriteSheet.Animation()
            {
                StartFrame = (5, 0),
                EndFrame = (5, 3),
                DurationMs = 1000,
                Loop = true
            };

            catSheet.Animations["LeftMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 1),
                EndFrame = (3, 3),
                DurationMs = 1000,
                Loop = true
            };

            catSheet.Animations["RightMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 1),
                EndFrame = (1, 3),
                DurationMs = 1000,
                Loop = true
            };

            catSheet.Animations["UpMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 3),
                DurationMs = 1000,
                Loop = true
            };

            catSheet.Animations["DownMove"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 3),
                DurationMs = 1000,
                Loop = true
            };


            _dog = new DogObject(dogSheet, 200, 200);
            _cat = new CatObject(catSheet, 300, 200);
        }

        public void ProcessCompanion(bool up, bool down, bool left, bool right, double secsSinceLastFrame, int levelWidth, int levelHeight)
        {
            //---------------------------------------------------------------------------------------------------------
            bool addCompanionKey = _input.IsFKeyPressed();

            if (addCompanionKey && _player.nearACompanion(_dog) && !_player.GetHasCompanion())
            {
                _player.SetHasCompanion(true, _dog);
                _dog.SetWildAnimal(false);
                _dog.SetFollowingPosition(_player.Position.X, _player.Position.Y);
            }
            else if (addCompanionKey && _player.nearACompanion(_cat) && !_player.GetHasCompanion())
            {
                _player.SetHasCompanion(true, _cat);
                _cat.SetWildAnimal(false);
                _cat.SetFollowingPosition(_player.Position.X, _player.Position.Y);
            }

            if (!_dog.IsWildAnimal())
            {
                _dog.SwitchAnimations(up, down, left, right);
                _dog.UpdateCompanionPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);
                _dog.SetDirection(left ? -1 : right ? 1 : 0, up ? 1 : down ? -1 : 0);
                _dog.SetFollowingPosition(_player.Position.X, _player.Position.Y);
            }

            if (!_cat.IsWildAnimal())
            {
                _cat.SwitchAnimations(up, down, left, right);
                _cat.UpdateCompanionPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);
                _cat.SetDirection(left ? -1 : right ? 1 : 0, up ? 1 : down ? -1 : 0);
                _cat.SetFollowingPosition(_player.Position.X, _player.Position.Y);
            }

            //---------------------------------------------------------------------------------------------------------

            // if the player press again F when a companion is following him, the companion becomes wild animal

            bool removeCompanionKey = _input.IsXKeyPressed();

            if (removeCompanionKey && _player.GetHasCompanion())
            {
                _player.GetCompanion().SetWildAnimal(true);
                _player.SetHasCompanion(false, null);

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

            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            ProcessCompanion(up, down, left, right, secsSinceLastFrame, _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight);

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
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
            _dog.Render(_renderer);
            _cat.Render(_renderer);
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