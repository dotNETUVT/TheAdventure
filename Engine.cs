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

        private int _currentBombCount = 0;
        private const int MaxBombCount = 5;
        private double _timeSinceLastBombRecharge = 0.0;

        private DateTimeOffset _lastMoveStartTime;
        private bool _isCurrentlyMoving;
        private bool _isCurrentlySliding;
        private double _currentSpeedMultiplier = 1.0;
        private bool _wasMovingFast = false;
        private DateTimeOffset _slidingStartTime;
        private double _lastUp, _lastDown, _lastLeft, _lastRight;
        private bool _alreadyPressed = false;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);
        }

        public void InitializeWorld()
        {   
            _input.InitializeControllers();
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
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            _timeSinceLastBombRecharge += secsSinceLastFrame;
            if (_timeSinceLastBombRecharge >= 3.0 && _currentBombCount < MaxBombCount)
            {
                _currentBombCount++;
                _timeSinceLastBombRecharge -= 3.0;
            }

            bool up = _input.IsUpPressed() || _input.IsWPressed() || _input.IsJoystickUpPressed();
            bool down = _input.IsDownPressed() || _input.IsSPressed() || _input.IsJoystickDownPressed();
            bool left = _input.IsLeftPressed() || _input.IsAPressed() || _input.IsJoystickLeftPressed();
            bool right = _input.IsRightPressed() || _input.IsDPressed() || _input.IsJoystickRightPressed();

            bool isMovementKeyPressed = up || down || left || right;

            UpdateMovementState(isMovementKeyPressed);

            if (!isMovementKeyPressed && _wasMovingFast)
            {   
                if (!_isCurrentlySliding)
                {
                    _slidingStartTime = DateTimeOffset.UtcNow;
                    _isCurrentlySliding = true;
                }

                if ((DateTimeOffset.UtcNow - _slidingStartTime).TotalSeconds <= 0.25)
                {
                    _player.UpdatePlayerPosition(_lastUp, _lastDown, _lastLeft, _lastRight,
                        _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                        secsSinceLastFrame * _currentSpeedMultiplier);
                }
                else
                {
                    _wasMovingFast = false;
                    _isCurrentlySliding = false;
                    _currentSpeedMultiplier = 1.0;
                }
            }
            else
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame * _currentSpeedMultiplier);

                _lastUp = up ? 1.0 : 0.0;
                _lastDown = down ? 1.0 : 0.0;
                _lastLeft = left ? 1.0 : 0.0;
                _lastRight = right ? 1.0 : 0.0;
            }

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            var controllerButtonPressed = _input.IsAButtonPressed() || _input.IsBButtonPressed() || _input.IsXButtonPressed() || _input.IsYButtonPressed();

            if(controllerButtonPressed)
            {   
                if (!_alreadyPressed)
                {
                    AddBomb(_player.Position.X, _player.Position.Y);
                    _alreadyPressed = true;
                }
            }
            else
            {
                _alreadyPressed = false;
            }

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        private void UpdateMovementState(bool isMovementKeyPressed)
        {
            if (isMovementKeyPressed && !_isCurrentlyMoving)
            {
                _lastMoveStartTime = DateTimeOffset.UtcNow;
                _isCurrentlyMoving = true;
            }
            else if (!isMovementKeyPressed && _isCurrentlyMoving)
            {
                _isCurrentlyMoving = false;
                _wasMovingFast = true;
                _currentSpeedMultiplier = 1.5;
            }

            if (_isCurrentlyMoving)
            {
                TimeSpan movementDuration = DateTimeOffset.UtcNow - _lastMoveStartTime;
                if (movementDuration.TotalSeconds > 2)
                {
                    _currentSpeedMultiplier = 2.0;  // Double the speed after 2 seconds
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

        private void AddBomb(int x, int y)
        {
            if (_currentBombCount <= 0)
            {
                return;
            }

            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if(spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);

                _currentBombCount--;
            }
        }
    }
}