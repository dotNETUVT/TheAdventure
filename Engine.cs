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
        private SpriteSheet? _wallSpriteSheet;

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
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
            GenerateMaze(level);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
            }
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));

            InitializeEnemies();
            LoadWallSpriteSheet();
        }

        private void LoadWallSpriteSheet()
        {
            _wallSpriteSheet = SpriteSheet.LoadSpriteSheet("wall.json", "Assets", _renderer);
            if (_wallSpriteSheet == null)
            {
                Console.WriteLine("Failed to load wall sprite sheet.");
            }
        }

        private void GenerateMaze(Level level)
        {
            Random random = new Random();
            double wallProbability = 0.1;

            for (int i = 0; i < level.Width; i++)
            {
                for (int j = 0; j < level.Height; j++)
                {
                    if (random.NextDouble() < wallProbability)
                    {
                        level.Layers[0].Data[j * level.Width + i] = 1;
                    }
                    else
                    {
                        level.Layers[0].Data[j * level.Width + i] = 2;
                    }
                }
            }
        }


        public bool CheckCollision(int newX, int newY)
        {
            int tileWidth = _currentLevel.TileWidth;
            int tileHeight = _currentLevel.TileHeight;
            int tileX = newX / tileWidth;
            int tileY = newY / tileHeight;

            return _currentLevel.Layers[0].Data[tileY * _currentLevel.Width + tileX] == 1;
        }

        private void InitializeEnemies()
        {
            int numberOfEnemies = 5;
            Random random = new Random();

            for (int i = 0; i < numberOfEnemies; i++)
            {
                int enemyX, enemyY;
                do
                {
                    enemyX = random.Next(0, _currentLevel.Width * _currentLevel.TileWidth);
                    enemyY = random.Next(0, _currentLevel.Height * _currentLevel.TileHeight);
                }
                while (CheckCollision(enemyX, enemyY));

                var spriteSheetEnemy = SpriteSheet.LoadSpriteSheet("enemy.json", "Assets", _renderer);
                if (spriteSheetEnemy != null)
                {
                    EnemyObject enemy = new EnemyObject(spriteSheetEnemy, enemyX, enemyY);
                    _gameObjects.Add(enemy.Id, enemy);
                }
                else
                {
                    Console.WriteLine("Failed to load enemy sprite sheet.");
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

            if (isAttacking)
            {
                ProcessAttack(up, down, left, right);
            }
            else
            {
                ProcessMovement(up, down, left, right, secsSinceLastFrame);
            }

            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, up, down, left, right, false);
            }

            UpdateEnemies(secsSinceLastFrame);

            CleanupExpiredGameObjects();
        }

        private void UpdateEnemies(double deltaTime)
        {
            var enemies = _gameObjects.Values.OfType<EnemyObject>().ToList();
            foreach (var enemy in enemies)
            {
                enemy.UpdatePosition(_currentLevel, _gameObjects, _renderer);
            }
        }

        private void ProcessAttack(bool up, bool down, bool left, bool right)
        {
            int directionCount = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
            if (directionCount <= 1)
            {
                _player.Attack(up, down, left, right);
                CheckEnemiesInAttackRange(up, down, left, right);
            }
        }

        private void CheckEnemiesInAttackRange(bool up, bool down, bool left, bool right)
        {
            int attackRange = 32;

            var attackArea = new Rectangle<int>(
                new Vector2D<int>(
                    _player.Position.X - (left ? attackRange : 0) + (right ? 0 : -attackRange),
                    _player.Position.Y - (up ? attackRange : 0) + (down ? 0 : -attackRange)),
                new Vector2D<int>(left || right ? attackRange * 2 : attackRange, up || down ? attackRange * 2 : attackRange)
            );

            foreach (var gameObject in _gameObjects.Values.OfType<EnemyObject>().ToList())
            {
                var enemyPosition = new Rectangle<int>(
                    new Vector2D<int>(gameObject.Position.X, gameObject.Position.Y),
                    new Vector2D<int>(gameObject.SpriteSheet.FrameWidth, gameObject.SpriteSheet.FrameHeight)
                );

                if (Intersects(attackArea, enemyPosition))
                {
                    gameObject.MarkForRemoval();
                }
            }
        }

        private void ProcessMovement(bool up, bool down, bool left, bool right, double deltaTime)
        {
            if (up || down || left || right)
            {
                _player.UpdatePlayerPosition(
                    up ? 1.0 : 0.0,
                    down ? 1.0 : 0.0,
                    left ? 1.0 : 0.0,
                    right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth,
                    _currentLevel.Height * _currentLevel.TileHeight,
                    deltaTime,
                    CheckCollision
                );
            }
        }

        private void CleanupExpiredGameObjects()
        {
            var itemsToRemove = new List<int>();
            foreach (var gameObject in GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired))
            {
                itemsToRemove.Add(gameObject.Id);
                if (gameObject is TemporaryGameObject tempObject && IsPlayerNear(tempObject))
                {
                    _player.GameOver();
                }
            }

            foreach (var gameObjectId in itemsToRemove)
            {
                _gameObjects.Remove(gameObjectId);
            }

            foreach (var enemy in _gameObjects.Values.OfType<EnemyObject>().Where(e => e.IsDead).ToList())
            {
                if (enemy.SpriteSheet.IsAnimationFinished())
                {
                    _gameObjects.Remove(enemy.Id);
                }
            }
        }

        private bool IsPlayerNear(TemporaryGameObject tempObject)
        {
            var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
            var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
            return deltaX < 32 && deltaY < 32;
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

            RenderWalls();
        }

        private void RenderWalls()
        {
            if (_wallSpriteSheet == null || _currentLevel == null) return;

            var wallSrc = new Rectangle<int>(0, 0, _wallSpriteSheet.FrameWidth, _wallSpriteSheet.FrameHeight);

            for (int i = 0; i < _currentLevel.Width; i++)
            {
                for (int j = 0; j < _currentLevel.Height; j++)
                {
                    if (_currentLevel.Layers[0].Data[j * _currentLevel.Width + i] == 1)
                    {
                        var halfWidth = _wallSpriteSheet.FrameWidth / 2;
                        var halfHeight = _wallSpriteSheet.FrameHeight / 2;
                        var wallDst = new Rectangle<int>(i * halfWidth, j * halfHeight, halfWidth, halfHeight);
                        _renderer.RenderTexture(_wallSpriteSheet.TextureId, wallSrc, wallDst);
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

        private void AddBomb(int x, int y, bool up, bool down, bool left, bool right, bool translateCoordinates = true)
        {
            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new TemporaryGameObject(
                    spriteSheet, 2.1, (translated.X, translated.Y), HandleExplosion
                );
                _gameObjects.Add(bomb.Id, bomb);
                CheckEnemiesInAttackRange(up, down, left, right);
            }
        }

        private void HandleExplosion(TemporaryGameObject bomb)
        {
            int blastRadius = 64;

            var affectedArea = new Rectangle<int>(
                new Vector2D<int>(bomb.Position.X - blastRadius, bomb.Position.Y - blastRadius),
                new Vector2D<int>(2 * blastRadius, 2 * blastRadius)
            );

            foreach (var gameObject in _gameObjects.Values.ToList())
            {
                if (gameObject is EnemyObject enemy)
                {
                    var enemyPosition = new Rectangle<int>(
                        new Vector2D<int>(enemy.Position.X, enemy.Position.Y),
                        new Vector2D<int>(enemy.SpriteSheet.FrameWidth, enemy.SpriteSheet.FrameHeight)
                    );

                    if (Intersects(affectedArea, enemyPosition))
                    {
                        enemy.MarkForRemoval();
                    }
                }
            }
        }

        public static bool Intersects(Rectangle<int> a, Rectangle<int> b)
        {
            var aMax = new Vector2D<int>(a.Origin.X + a.Size.X, a.Origin.Y + a.Size.Y);
            var bMax = new Vector2D<int>(b.Origin.X + b.Size.X, b.Origin.Y + b.Size.Y);

            return a.Origin.X < bMax.X &&
                   aMax.X > b.Origin.X &&
                   a.Origin.Y < bMax.Y &&
                   aMax.Y > b.Origin.Y;
        }
    }
}
