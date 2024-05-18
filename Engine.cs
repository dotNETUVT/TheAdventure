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
        private Random _random = new Random();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;
        private IntPtr _diamondCaughtSound;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;
        private int _diamondsCaught = 0; // Diamond counter

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
            _renderer.LoadSoundEffect("Assets/collect_diamond.mp3", out _diamondCaughtSound);

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
            
            // Place 5 diamonds at random positions
            for (int i = 0; i < 15; i++)
            {
                var (x, y) = GetRandomPosition(_currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight);
                AddDiamond(x, y);
            }
        }
        
        private (int X, int Y) GetRandomPosition(int maxX, int maxY)
        {
            int x = _random.Next(0, maxX);
            int y = _random.Next(0, maxY);
            return (x, y);
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

            if(isAttacking)
            {
                var dir = up ? 1: 0;
                dir += down? 1 : 0;
                dir += left? 1: 0;
                dir += right ? 1 : 0;
                if(dir <= 1){
                    _player.Attack(up, down, left, right);
                }
                else{
                    isAttacking = false;
                }
            }
            if(!isAttacking)
            {
                _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                    _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                    secsSinceLastFrame);
            }
            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());
            
            // Check for collision with diamonds
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject renderableGameObject &&
                    renderableGameObject.SpriteSheet.FileName == "diamond.png")
                {
                    if (IsColliding(_player, renderableGameObject))
                    {
                        itemsToRemove.Add(renderableGameObject.Id);
                        _diamondsCaught++;
                        _renderer.PlaySoundEffect("Assets/collect_diamond.mp3");
                    }
                }
            }

            if (addBomb)
            {
                AddBomb(_player.Position.X, _player.Position.Y, false);
            }

            foreach (var gameObjectId in itemsToRemove)
            {
                var gameObject = _gameObjects[gameObjectId];
                if(gameObject is TemporaryGameObject){
                    var tempObject = (TemporaryGameObject)gameObject;
                    var deltaX = Math.Abs(_player.Position.X - tempObject.Position.X);
                    var deltaY = Math.Abs(_player.Position.Y - tempObject.Position.Y);
                    if(deltaX < 32 && deltaY < 32){
                        _player.GameOver();
                    }
                }
                _gameObjects.Remove(gameObjectId);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();
            
            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();
            
            // Render the diamond counter
            RenderDiamondCounter();

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

        private void AddBomb(int x, int y, bool translateCoordinates = true)
        {

            var translated = translateCoordinates ? _renderer.TranslateFromScreenToWorldCoordinates(x, y) : new Vector2D<int>(x, y);
            
            var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", _renderer);
            if(spriteSheet != null){
                spriteSheet.ActivateAnimation("Explode");
                TemporaryGameObject bomb = new(spriteSheet, 2.1, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }
        }
        
        public void AddDiamond(int x, int y)
        {
            var spriteSheet = SpriteSheet.LoadSpriteSheet("diamond.json", "Assets", _renderer);
            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Shine");
                var diamond = new RenderableGameObject(spriteSheet, (x, y));
                _gameObjects.Add(diamond.Id, diamond);
            }
        }
        
        private bool IsColliding(PlayerObject player, RenderableGameObject diamond)
        {
            var playerRect = new Rectangle<int>(
                player.Position.X - player.SpriteSheet.FrameCenter.OffsetX,
                player.Position.Y - player.SpriteSheet.FrameCenter.OffsetY,
                player.SpriteSheet.FrameWidth,
                player.SpriteSheet.FrameHeight
            );

            var diamondRect = new Rectangle<int>(
                diamond.Position.X - diamond.SpriteSheet.FrameCenter.OffsetX,
                diamond.Position.Y - diamond.SpriteSheet.FrameCenter.OffsetY,
                diamond.SpriteSheet.FrameWidth,
                diamond.SpriteSheet.FrameHeight
            );

            return playerRect.Overlaps(diamondRect);
        }
        
        private void RenderDiamondCounter()
{
    // Load the digits' textures
    var digitsTextureIds = new List<int>();
    foreach (var digitChar in _diamondsCaught.ToString())
    {
        var digitFilePath = $"Assets/Numbers/Number{digitChar}.png";
        var digitTextureId = _renderer.LoadTexture(digitFilePath, out _);
        digitsTextureIds.Add(digitTextureId);
    }

    // Load the first frame of the diamond sprite sheet
    var diamondSheetFilePath = "diamond.json"; 
    var diamondSpriteSheet = SpriteSheet.LoadSpriteSheet(diamondSheetFilePath, "Assets", _renderer);
    var diamondTextureId = diamondSpriteSheet?.GetTextureId(0); // Get the texture ID of the first frame
    
    // Determine the position to render the diamond and the counter (relative to the camera)
    var counterPosition = new Vector2D<int>(10, 10); 
    counterPosition += _renderer.TranslateFromScreenToWorldCoordinates(0, 0); 

    var diamondPosition = new Vector2D<int>(counterPosition.X + 30, counterPosition.Y - 10); 

    // Render each digit
    var digitWidth = 10; 
    foreach (var textureId in digitsTextureIds)
    {
        var srcRect = new Rectangle<int>(0, 0, digitWidth, 14); 
        var dstRect = new Rectangle<int>(counterPosition.X, counterPosition.Y, digitWidth, 14);
        _renderer.RenderTexture(textureId, srcRect, dstRect);

        counterPosition.X += digitWidth; 
    }

    // Render the diamond
    if (diamondTextureId.HasValue)
    {
        var srcRect = diamondSpriteSheet.GetFrameSourceRect(0); // Get the source rectangle of the first frame
        var dstRect = new Rectangle<int>(diamondPosition.X, diamondPosition.Y, srcRect.Size.X, srcRect.Size.Y);
        _renderer.RenderTexture(diamondTextureId.Value, srcRect, dstRect);
    }
}

    }
}