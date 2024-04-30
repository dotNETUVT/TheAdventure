using System.Security.Cryptography.X509Certificates;
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

            Random rnd = new Random();
            foreach (int x in Enumerable.Range(1, 58))
            {
                foreach (int y in Enumerable.Range(1, 38))
                {
                    if (_player.collide(x*16, y*16, 16, 16) == false)
                    {
                        //chance to generate an object
                        int value = rnd.Next(50);
                        if (value >= 5 && value <= 13)
                        {
                            // on 6 generate a house
                            if (value == 6)
                            {
                                value = rnd.Next(6);
                                AddHouse(x * 16, y * 16, value);
                            }
                            else if (rnd.Next(30) == 2) // even smaller chance to generate some extras
                            {
                                value = rnd.Next(4);
                                AddExtra(x * 16, y * 16, value);
                            }
                            else if (value % 2 == 1) // on odd number generate a tree
                            {
                                value = rnd.Next(5);
                                AddTree(x * 16, y * 16, value);
                            }
                            

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

            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            foreach (var gameObject in GetAllRenderableObjects())
            {
                var posX = gameObject.Position.X;
                var posY = gameObject.Position.Y;
                var hasColided = false;

                while (_player.collide(posX, posY, 16, 16) == true)
                {
                    /*Console.Out.Write(posX);
                    Console.Out.Write(" ");
                    Console.Out.Write(posY);
                    Console.Out.Write(" ");
                    Console.Out.Write(_player.Position.X);
                    Console.Out.Write(" ");
                    Console.Out.Write(_player.Position.Y);
                    Console.Out.Write(" ");
                    Console.Out.Write(_player.collide(posX, posY, 16, 16));
                    Console.Out.Write("\n");*/

                    var currAnimation = _player.getCurrentAnimation();
                    if (currAnimation == "MoveLeft")
                        _player.setPosition(1, 0);
                    else if (currAnimation == "MoveRight")
                        _player.setPosition(-1, 0);
                    else if (currAnimation == "MoveDown")
                        _player.setPosition(0, -1);
                    else if (currAnimation == "MoveUp")
                        _player.setPosition(0, 1);
                    hasColided = true;
                }

                if (hasColided)
                    break;
            }

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

        private void AddHouse(int x, int y, int tileNumber)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("house.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.selectTexture(0, tileNumber);
                RenderableGameObject house = new(spriteSheet, (translated.X, translated.Y));
                _gameObjects.Add(house.Id, house);
            }
        }

        private void AddTree(int x, int y, int tileNumber)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("tree.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.selectTexture(0, tileNumber);
                RenderableGameObject tree = new(spriteSheet, (translated.X, translated.Y));
                _gameObjects.Add(tree.Id, tree);
            }
        }

        private void AddExtra(int x, int y, int tileNumber)
        {
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("extra.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.selectTexture(0, tileNumber);
                RenderableGameObject extra = new(spriteSheet, (translated.X, translated.Y));
                _gameObjects.Add(extra.Id, extra);
            }
        }
    }
}
