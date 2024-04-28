using System.Text.Json;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace TheAdventure
{
    class MainMenu : GameScene
    {
        private MainMenuLevel? _currentLevel;

        private Dictionary<Scenes, GameScene> _loadedScenes = new();
        public MainMenu(GameRenderer renderer, Input input)
        : base(renderer, input, "Assets", "player.json", 400, 240)
        {
        }

        private readonly Interractable playButton = new(23, 19, 4, 4);
        private readonly Interractable exitButton = new(24, 1, 2, 3);

        private MainMenuLevel? LoadLevel()
        {
            var levelContent = File.ReadAllText(Path.Combine("Assets", "mainMenu", "the_adventure_main_menu.tmj"));

            var level = JsonSerializer.Deserialize<MainMenuLevel>(levelContent, JsonSerializerOptions);
            if (level == null) return null;
            foreach (var tileSet in level.TileSets)
            {
                _renderer.LoadMainMenuTileSet(tileSet);
            }

            return level;
        }

        public override void InitializeScene()
        {
            _currentLevel = LoadLevel();
            if (_currentLevel == null) return;

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                    _currentLevel.Height * _currentLevel.TileHeight));

            InitializePlayer();
        }

        public override void DeInitializeScene()
        {
            if (_currentLevel == null) return;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Tiles)
                {
                    _renderer.UnloadTexture(tile.InternalTextureId);
                }
                tileSet.Tiles = [];
            }

            _currentLevel = null;
            DisposePlayer();
            DeActivate();
        }

        public override bool ProcessFrame()
        {
            if(!IsActive()) return false;
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();

            GetPlayer()?.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                        _currentLevel!.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                        secsSinceLastFrame);

            if(_input.IsEnterPressed())
            {
                var (X, Y) = GetPlayer()!.Position;
                if(playButton.IsObjectInteracted((int)X, (int)Y))
                {
                    DeInitializeScene();
                    _loadedScenes[Scenes.simpleWorld].Activate();
                }
                else if(exitButton.IsObjectInteracted((int)X, (int)Y))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Render()
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
                        if (cTile == null)
                        {
                            continue;
                        }

                        var src = new Rectangle<int>(0, 0, cTile.ImageWidth, cTile.ImageHeight);
                        var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight, cTile.ImageWidth,
                            cTile.ImageHeight);

                        _renderer.RenderTexture(cTile.InternalTextureId, src, dst);
                    }
                }
            }
        }

        protected override Tile? GetTile(int id)
        {
            if (_currentLevel == null) return null;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Tiles)
                {
                    if (tile.Id == id)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }
        public void AddScene(Scenes scene, GameScene gameScene)
        {
            _loadedScenes.Add(scene, gameScene);
        }

        public void RemoveScene(Scenes scene)
        {
            _loadedScenes.Remove(scene);
        }
    }
}
