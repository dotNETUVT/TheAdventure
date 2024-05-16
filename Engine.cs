using System;
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

        // Creating Portals
        private Portal? portal1 = null;
        private Portal? portal2 = null;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPortalSpawn = DateTimeOffset.Now.AddSeconds(-8);

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnMouseClick += (_, coords) =>AddBomb(coords.x, coords.y);
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

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();
            bool isSpawningPortal = _input.IsPKeyPressed();

            int portalCooldown = 1;
            bool portalAvailiable = (currentTime - _lastPortalSpawn).TotalSeconds > portalCooldown;
            if (isSpawningPortal && portalAvailiable)
            {
                AddPortal();
                _lastPortalSpawn = currentTime;
            }
            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            var PCheck = CheckPortal(_player.Position.X, _player.Position.Y);
            if (PCheck.Item1 == true)
            {
                _player.PortalTeleport(PCheck.Item2, PCheck.Item3, _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);
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


        private void AddPortal()
        {
            (int maxW, int maxH) = (_currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight);
            bool two = false;
            (int x, int y) = (0, 0);
            Random r = new Random();
            do
            {
                x = r.Next(maxW);
                y = r.Next(maxH);
            } while (x < 100 && x + 100 > maxW && y < 100 && y + 100 > maxH);

            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);

            var spriteSheet = SpriteSheet.LoadSpriteSheet("portal.json", "Assets", _renderer);
            //1.Check how many portals are there
            //If 2 closes the first one

            //2. Randomizes an x and y positions

            //3. Add the portal to those random positions and add to the list
            if ( portal1 == null)
            {
                portal1 = new Portal(spriteSheet,x,y);
                portal1.setPortalAsFirt(ref portal1 ); 
            }
            else if (portal2 == null)
            {             
                portal2 = new Portal(spriteSheet, x, y);
                portal2.LinkAnotherPortal(ref portal1,ref portal2);
                two=true; 
            }
            else 
            {
                //Delete the first one and transform the second into first
                _gameObjects.Remove(portal1.Id);
                portal1 = portal2;
                portal1.setPortalAsFirt(ref portal1);
                portal2 = new(spriteSheet, x, y);
                two = true;

            }





            if (spriteSheet != null)
            {
                spriteSheet.ActivateAnimation("Idle");
                TemporaryGameObject portal = new(spriteSheet, 2.1, (translated.X, translated.Y));

                if (two == false) { 
                _gameObjects.Add(portal1.Id, portal1);
                }
                else
                {
                _gameObjects.Add(portal2.Id, portal2);
                }
            }

        }
       
        
        private (bool,int, int) CheckPortal(int x, int y)
        {
            var spriteSheet = SpriteSheet.LoadSpriteSheet("portal.json", "Assets", _renderer);
            if (portal2 == null || portal1 == null)
                return (false,x, y);
            else
            {
                if (Math.Abs(x - portal1.Position.X) <15 && Math.Abs(y - portal1.Position.Y) < 15)
                {
                    spriteSheet.ActivateAnimation("Spawn");
                    TemporaryGameObject portal = new(spriteSheet, 2.1, (x, y));
                    return (true, portal2.Position.X, portal2.Position.Y);
                }
                else if (Math.Abs(x - portal2.Position.X) <15 && Math.Abs(y - portal2.Position.Y) < 15)
                {
                    spriteSheet.ActivateAnimation("Spawn");
                    TemporaryGameObject portal = new(spriteSheet, 2.1, (x, y));
                    return (true, portal1.Position.X, portal1.Position.Y);
                }
            }
            return (false,x, y);
        }

    }


}