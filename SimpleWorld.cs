using System.Text.Json;
using Silk.NET.Maths;

namespace TheAdventure
{
    class SimpleWorld : GameScene
    {
        private Level? _currentLevel;

        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private readonly MainMenu _mainMenuScene;

        public SimpleWorld(GameRenderer renderer, Input input, MainMenu mainMenuScene)
        : base(renderer, input, "Assets", "player.json", 100, 100)
        {
            _mainMenuScene = mainMenuScene;
        }

        public override void InitializeScene()
        {
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, JsonSerializerOptions);
            if (level == null) return;
            foreach (var refTileSet in level.TileSets)
            {
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if (!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet))
                {
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, JsonSerializerOptions);

                    foreach (var tile in tileSet!.Tiles)
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
            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));

            InitializePlayer();
        }

        public override void DeInitializeScene()
        {
            if (_currentLevel == null) return;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Set.Tiles)
                {
                    _renderer.UnloadTexture(tile.InternalTextureId);
                }
                tileSet.Set.Tiles = [];
            }
            _currentLevel = null;
            _loadedTileSets.Clear();

            _mainMenuScene.Activate();
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

            GetPlayer()!.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel!.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);

            if (_input.IsEscPressed())
            {
                DeActivate();
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
                        if (cTile == null) continue;

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
    }
}
