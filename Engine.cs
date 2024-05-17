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

        private bool _isStartScreenVisible = true;
        private DateTimeOffset _startScreenTime = DateTimeOffset.Now.AddSeconds(2);

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
            InitializeWorld();
            InitializeStartScreen(); 
            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y, true);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", _renderer);
            if(spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, 100, 100);
                _gameObjects.Add(_player.Id, _player);
            }
        }

        private void InitializeStartScreen()
        {
            _isStartScreenVisible = true;
            _startScreenTime = DateTimeOffset.Now.AddSeconds(2); // The start screen will be displayed for 2 seconds
        }

        public void InitializeWorld()
        {
            // World initialization logic here...
        }

        private void CheckPlayerBombCollisions()
        {
            // Collision checking logic here...
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            if (_isStartScreenVisible && currentTime >= _startScreenTime)
            {
                _isStartScreenVisible = false; // Stop displaying the start screen after 2 seconds
            }
            CheckPlayerBombCollisions();

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
                var dir = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
                if(dir == 1){
                    _player?.Attack(up, down, left, right);
                }
                else{
                    isAttacking = false;
                }
            }
            if(!isAttacking && _player != null)
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel?.Width * _currentLevel.TileWidth ?? 0, _currentLevel?.Height * _currentLevel.TileHeight ?? 0,
                    secsSinceLastFrame);
            }
            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, false);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();
            // Rendering logic for start screen and game objects...
        }

        private void AddBomb(int x, int y, bool translateCoordinates)
        {
            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);
            
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if(spriteSheet != null){
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
    }
}
