using TheAdventure.Models;
using System.Text.Json;

namespace TheAdventure
{
    abstract class GameScene(GameRenderer renderer, Input input, String playerSpriteSheetFolder, String playerSpriteSheetFilename, int playerSpawnPozitionX, int playerSpawnPozitionY)
    {
        protected readonly GameRenderer _renderer = renderer;
        protected readonly Input _input = input;

        protected DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        protected DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        private readonly string _playerSpriteSheetFolder = playerSpriteSheetFolder;
        private readonly string _playerSpriteSheetFilename = playerSpriteSheetFilename;

        private readonly int _playerSpawnPozitionX = playerSpawnPozitionX;
        private readonly int _playerSpawnPozitionY = playerSpawnPozitionY;
        private PlayerObject? _player;

        protected JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        private bool _isActive = false;

        protected void InitializePlayer()
        {
            var spriteSheet = SpriteSheet.LoadSpriteSheet(_playerSpriteSheetFilename, _playerSpriteSheetFolder, _renderer);
            if(spriteSheet != null)
            {
                _player = new PlayerObject(spriteSheet, _playerSpawnPozitionX, _playerSpawnPozitionY);
            }
        }
        public abstract void InitializeScene();
        public abstract void DeInitializeScene();

        public abstract bool ProcessFrame();

        protected abstract void Render();

        protected abstract Tile? GetTile(int id);

        public void RenderFrame()
        {
            if(!_isActive) return;
            Render();
            _renderer.CameraLookAt(_player!.Position.X, _player!.Position.Y);

            _player.Render(_renderer);
        }

        public PlayerObject? GetPlayer()
        {
            return _player;
        }

        public void DisposePlayer()
        {
            _player = null;
        }

        public void Activate()
        {
            _isActive = true;
            InitializeScene();
        }

        public void DeActivate()
        {
            _isActive = false;
            DeInitializeScene();
        }

        public bool IsActive()
        {
            return _isActive;
        }
    }
}
