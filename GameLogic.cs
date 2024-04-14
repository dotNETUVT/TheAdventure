using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        private GirlObject _girl;

        public GameLogic()
        {
            
        }

        public void LoadGameState()
        {
            
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
            _player = new PlayerObject(1000, 24, 24, _currentLevel.TileWidth * _currentLevel.Width, _currentLevel.TileHeight * _currentLevel.Height);
            _girl = new GirlObject(900, 100, 100, _currentLevel.TileWidth * _currentLevel.Width, _currentLevel.TileHeight * _currentLevel.Height);
        }

        public Rectangle<int> GetWorldBoundingBox(){
            if (_currentLevel == null) return new Rectangle<int>();
            return new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight);
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

        public void UpdatePlayerPosition(double up, double down, double left, double right, int timeSinceLastUpdateInMS)
        {

            _player.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);
            //_girl.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);

        }
        public void UpdatePrincessPosition(double up, double down, double left, double right, int timeSinceLastUpdateInMS)
        {

            _girl.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);

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
            _girl.Render(renderer);
        }

        private int _bombIds = 100;

        /// <summary>
        /// Add a bomb at the given coordinates.
        /// </summary>
        /// <remarks>
        /// <paramref name="x"/> and <paramref name="y"/> are in screen coordinates.
        /// </remarks>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddBomb(int x, int y, GameRenderer renderer)
        {
            var translated = renderer.TranslateFromScreenToWorldCoordinates(x, y);
           // Console.WriteLine($"Translated Coordinates: ({translated.X}, {translated.Y})");
            AnimatedGameObject bomb = new AnimatedGameObject("BombExploding.png", 2, _bombIds, 13, 13, 1, translated.X, translated.Y);
            _gameObjects.Add(bomb.Id, bomb);
            ++_bombIds;
        }
    }
}