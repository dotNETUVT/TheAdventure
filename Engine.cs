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
        private PlayerObject _player1;
        private PlayerObject _player2;  // Add second player
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

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
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                _player1 = new PlayerObject(spriteSheet, 100, 100);
                _player2 = new PlayerObject(spriteSheet, 200, 200);  // Initialize second player
            }

            // Initialize potion
            var potionSpriteSheet = SpriteSheet.LoadSpriteSheet("potion.json", "Assets", _renderer);
            if (potionSpriteSheet != null)
            {
                var potion = new PotionObject(potionSpriteSheet, 200, 200);
                _gameObjects.Add(potion.Id, potion);
            }

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            ProcessPlayerInput(_player1, _input, secsSinceLastFrame);
            ProcessPlayerInput(_player2, _input, secsSinceLastFrame, playerIndex: 2);

            CheckPotionCollision();
            RemoveExpiredObjects();
        }

        private void ProcessPlayerInput(PlayerObject player, Input input, double secsSinceLastFrame, int playerIndex = 1)
        {
            bool up = playerIndex == 1 ? input.IsUpPressed() : input.IsKeyWPressed();
            bool down = playerIndex == 1 ? input.IsDownPressed() : input.IsKeySPressed();
            bool left = playerIndex == 1 ? input.IsLeftPressed() : input.IsKeyAPressedPlayer2();
            bool right = playerIndex == 1 ? input.IsRightPressed() : input.IsKeyDPressed();
            bool isAttacking = playerIndex == 1 ? input.IsKeyAPressed() : input.IsKeyJPressed();
            bool addBomb = playerIndex == 1 ? input.IsKeyBPressed() : input.IsKeyKPressed();

            if (isAttacking)
            {
                var dir = up ? 1 : 0;
                dir += down ? 1 : 0;
                dir += left ? 1 : 0;
                dir += right ? 1 : 0;
                if (dir <= 1)
                {
                    player.Attack(up, down, left, right);
                }
                else
                {
                    isAttacking = false;
                }
            }
            if (!isAttacking)
            {
                player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }

            if (addBomb)
            {
                AddBomb(player.Position.X, player.Position.Y, false);
            }
        }

        private void CheckPotionCollision()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is PotionObject potion)
                {
                    if (IsColliding(_player1, potion))
                    {
                        _player1.DrinkPotion();
                        _gameObjects.Remove(potion.Id);
                        break;
                    }
                    if (IsColliding(_player2, potion))
                    {
                        _player2.DrinkPotion();
                        _gameObjects.Remove(potion.Id);
                        break;
                    }
                }
            }
        }

        private void RemoveExpiredObjects()
        {
            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if (gameObject is TemporaryGameObject)
                {
                    var tempObject = (TemporaryGameObject)gameObject;
                    var deltaX = Math.Abs(_player1.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player1.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player1.GameOver();
                    }

                    deltaX = Math.Abs(_player2.Position.X - tempObject.Position.X);
                    deltaY = Math.Abs(_player2.Position.Y - tempObject.Position.Y);
                    if (deltaX < 32 && deltaY < 32)
                    {
                        _player2.GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player1.Position.X, _player1.Position.Y);

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

            _player1.Render(_renderer);
            _player2.Render(_renderer);  // Render second player
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

        private bool IsColliding(RenderableGameObject a, RenderableGameObject b)
        {
            var deltaX = Math.Abs(a.Position.X - b.Position.X);
            var deltaY = Math.Abs(a.Position.Y - b.Position.Y);
            return deltaX < 32 && deltaY < 32;
        }
    }
}
