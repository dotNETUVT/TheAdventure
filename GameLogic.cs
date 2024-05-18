using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace TheAdventure
{
    public class GameLogic
    {
        private Dictionary<int, GameObject> _gameObjects = new();
        private Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private PlayerObject _player2;
        private int _mode;

        public GameLogic()
        {

        }

        public void LoadGameState()
        {
            _player = new PlayerObject(1000);
            _player2 = new PlayerObject(2000, 30, 0);
            _player.active = true;
            _player2.active = false;
            var jsonSerializerOptions =  new JsonSerializerOptions(){ PropertyNameCaseInsensitive = true };
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, jsonSerializerOptions);
            if(level == null) return;
            foreach(var refTileSet in level.TileSets){
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if(!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet)){
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, jsonSerializerOptions);

                    foreach(var tile in tileSet.Tiles)
                    {
                        var internalTextureId = GameRenderer.LoadTexture(Path.Combine("Assets", tile.Image), out var _);
                        tile.InternalTextureId = internalTextureId;
                    }

                    _loadedTileSets[refTileSet.Source] = tileSet;
                }
                refTileSet.Set = tileSet;
            }
            _currentLevel = level;
            this._mode = 0;
        }

        public IEnumerable<RenderableGameObject> GetAllRenderableObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject)
                {
                    yield return (RenderableGameObject)gameObject;
                }
            }
        }

        public void ProcessFrame()
        {
        }

        public Tile? GetTile(int id)
        {
            if (_currentLevel == null) return null;
            foreach(var tileSet in _currentLevel.TileSets){
                foreach(var tile in tileSet.Set.Tiles)
                {
                    if(tile.Id == id)
                    {
                        return tile;
                    }
                }
            }
            return null;
        }

        public void setMode(int mode)
        {
            this._mode = mode;
        }
        public void UpdatePlayerPosition(double up, double down, double left, double right, int timeSinceLastUpdateInMS)
        {
            
            if ( _player.active)
            {
                _player.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);
                if (((_player.X - 6 <= _player2.X && _player.X >= _player2.X) && (_player.Y + 15 >= _player2.Y && _player.Y <= _player2.Y)) ||
                    ((_player.X - 6 <= _player2.X && _player.X >= _player2.X) && (_player.Y - 15 <= _player2.Y && _player.Y >= _player2.Y)) ||
                    ((_player.X + 6 >= _player2.X && _player.X <= _player2.X) && (_player.Y + 15 >= _player2.Y && _player.Y <= _player2.Y)) ||
                    ((_player.X + 6 >= _player2.X && _player.X <= _player2.X) && (_player.Y - 15 <= _player2.Y && _player.Y >= _player2.Y)))
                {
                    

                    _player.undoUpdate(up, down, left, right, timeSinceLastUpdateInMS);
                    
                }
            } else if ( _player2.active) 
            {
                _player2.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);
                if (((_player2.X - 6 <= _player.X && _player2.X > _player.X) && (_player2.Y + 12 >= _player.Y && _player2.Y < _player.Y)) ||
                    ((_player2.X - 6 <= _player.X && _player2.X > _player.X) && (_player2.Y - 12 <= _player.Y && _player2.Y > _player.Y)) ||
                    ((_player2.X + 6 >= _player.X && _player2.X < _player.X) && (_player2.Y + 12 >= _player.Y && _player2.Y < _player.Y)) ||
                    ((_player2.X + 6 >= _player.X && _player2.X < _player.X) && (_player2.Y - 12 <= _player.Y && _player2.Y > _player.Y)))
                {
                    _player2.undoUpdate(up, down, left, right, timeSinceLastUpdateInMS);
                }
            }
            
            
        }

        public (int x, int y) GetPlayerCoordinates()
        {
            return (_player.X, _player.Y);
        }

        public void RenderTerrain(GameRenderer renderer)
        {
            if (_currentLevel == null) return;
            for(var layer = 0; layer < _currentLevel.Layers.Length; ++layer){
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j){
                        var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                        var cTile = GetTile(cTileId);
                        if(cTile == null) continue;
                        
                        var src = new Rectangle<int>(0,0, cTile.ImageWidth, cTile.ImageHeight);
                        var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight, cTile.ImageWidth, cTile.ImageHeight);

                        renderer.RenderTexture(cTile.InternalTextureId, src, dst);
                    }
                }
            }
        }

        public IEnumerable<RenderableGameObject> GetRenderables()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject)
                {
                    yield return (RenderableGameObject)gameObject;
                }
            }
        }

        public void RenderAllObjects(int timeSinceLastFrame, GameRenderer renderer)
        {
            List<int> itemsToRemove = new List<int>();
            foreach (var gameObject in GetAllRenderableObjects())
            {
                if (gameObject.Update(timeSinceLastFrame))
                {
                    gameObject.Render(renderer);
                }
                else
                {
                    itemsToRemove.Add(gameObject.Id);
                }
            }

            foreach (var item in itemsToRemove)
            {
                _gameObjects.Remove(item);
            }

            _player.Render(renderer);
            _player2.Render(renderer, 192, 48);
        }

        private int _bombIds = 100;
        private int _roseIds = 100;

        public void AddGameObject(int x, int y)
        {
            if (this._mode == 0) { AddBomb(x, y); }
            else if(this._mode == 1) {  AddRose(x, y); }
        }

        public void AddBomb(int x, int y)
        {
            AnimatedGameObject bomb = new AnimatedGameObject("BombExploding.png", 2, _bombIds, 13, 13, 1, x, y);
            _gameObjects.Add(bomb.Id, bomb);
            ++_bombIds;
        }

        public void AddRose(int x, int y)
        {
            AnimatedGameObject rose = new AnimatedGameObject("RoseBlooming.png", 3, _roseIds, 6, 6, 1, x, y);
            _gameObjects.Add(rose.Id, rose);
            ++_roseIds;
        }

        public PlayerObject getPlayer()
        {
            return _player;
        }

        public PlayerObject getPlayer2()
        {
            return _player2;
        }
    }
}