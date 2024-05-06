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

         private bool isGameOver = false;
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

            InitializeWorld(); // Adaugă această linie pentru a inițializa lumea jocului la crearea motorului

            // Inițializează și adaugă player-ul în lista de obiecte
            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if(spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
                _gameObjects.Add(_player.Id, _player);
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

        public bool IntersectsWith(PlayerObject player, TemporaryGameObject bomb)
        {
            // Coordonatele și dimensiunile dreptunghiului care reprezintă jucătorul
            int playerLeft = player.Position.X;
            int playerTop = player.Position.Y;
            int playerRight = player.Position.X + player.SpriteSheet.FrameWidth;
            int playerBottom = player.Position.Y + player.SpriteSheet.FrameHeight;

            // Coordonatele și dimensiunile dreptunghiului care reprezintă bomba
            int bombLeft = bomb.Position.X;
            int bombTop = bomb.Position.Y;
            int bombRight = bomb.Position.X + bomb.SpriteSheet.FrameWidth;
            int bombBottom = bomb.Position.Y + bomb.SpriteSheet.FrameHeight;

            // Verificăm dacă cele două dreptunghiuri se intersectează
            return playerLeft < bombRight && playerRight > bombLeft &&
                playerTop < bombBottom && playerBottom > bombTop;
        }

        

        private void CheckPlayerBombCollisions()
        {
            var playerBounds = new Rectangle<int>(_player.Position.X, _player.Position.Y,
                _player.SpriteSheet.FrameWidth, _player.SpriteSheet.FrameHeight);

            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject bomb)
                {
                    if (IntersectsWith(_player, bomb))
                    {
                        _gameObjects.Remove(_player.Id);
                        _player = null; // Actualizăm referința _player pentru a reflecta eliminarea acestuia din joc
                        break;
                    }
                }
            }
        }




        public void ProcessFrame()
        {
            CheckPlayerBombCollisions();

            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();

            // Utilizarea operatorului null-conditional pentru a apela metoda numai dacă _player nu este nul
            _player?.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel?.Width * _currentLevel.TileWidth ?? 0, _currentLevel?.Height * _currentLevel.TileHeight ?? 0,
                secsSinceLastFrame);

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
            
            // Verificăm dacă _player exista
            if (_player != null)
            {
                _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);
            }
            else
            {
                Console.WriteLine("Game over");
            }

            RenderTerrain();

            // Desenăm bomba chiar și după ce jucătorul a fost eliminat din joc
            foreach (var gameObject in GetAllTemporaryGameObjects())
            {
                gameObject.Render(_renderer);
            }

            // Desenăm restul obiectelor
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

            if (_player != null) // Asigură-te că player-ul este desenat doar dacă există
            {
                _player.Render(_renderer);
            }
        }



        private void AddBomb(int x, int y)
        {
            try
            {
                var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);

                var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
                if (spriteSheet != null)
                {
                    // Verificăm dacă jucătorul este inițializat înainte de a adăuga bomba
                    if (_player != null)
                    {
                        // Activăm animația "Explode" înainte de a crea obiectul bombei
                        spriteSheet.ActivateAnimation("Explode");

                        TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                        _gameObjects.Add(bomb.Id, bomb);
                    }
                    else
                    {
                        Console.WriteLine("Eroare: Jucătorul nu este inițializat.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la adăugarea bombei: {ex.Message}");
            }
        }

    }
}