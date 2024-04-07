using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Maths;

namespace TheAdventure
{
    /// <summary>
    /// Manages the core game logic, including loading game states, rendering, and updating game objects and the player.
    /// This class is responsible for orchestrating the high-level behavior of the game.
    /// </summary>
    public class GameLogic
    {
        /// <value>
        /// Stores all game objects, indexed by their unique ID. This dictionary serves as the central registry for all active game objects.
        /// </value>
        private Dictionary<int, GameObject> _gameObjects = new();
        
        /// <value>
        /// Stores tile sets loaded from external files, indexed by their source file name. This allows for efficient reuse of tile set data across levels.
        /// </value>
        private Dictionary<string, TileSet> _loadedTileSets = new();
        
        /// <value>
        /// Represents the currently active level. It can be null if no level is currently loaded. This allows the game to dynamically load and unload levels.
        /// </value>
        private Level? _currentLevel;
        
        /// <value>
        /// The player object, representing the user's character in the game. This object is central to player interactions and movements.
        /// </value>
        private PlayerObject _player;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameLogic"/> class, setting up the initial state for the game logic.
        /// </summary>
        public GameLogic()
        {
            
        }

        /// <summary>
        /// Loads the initial game state from external JSON files. This includes parsing level layouts, initializing tile sets, and setting the player's starting position.
        /// </summary>
        public void LoadGameState()
        {
            _player = new PlayerObject(1000);
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
        }

        /// <summary>
        /// Retrieves all renderable game objects currently active in the game.
        /// </summary>
        /// <returns>An enumerable collection of renderable game objects.</returns>
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

        
        /// <summary>
        /// Processes a single frame of game logic, updating the state of the game as necessary. This method should be called repeatedly in the game loop.
        /// </summary>
        public void ProcessFrame()
        {
        }

        /// <summary>
        /// Retrieves a specific tile based on its ID.
        /// </summary>
        /// <param name="id">The unique ID of the tile to retrieve.</param>
        /// <returns>The tile with the specified ID, or null if no such tile exists.</returns>
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

        /// <summary>
        /// Updates the player's position based on input directions and the amount of time that has passed since the last update.
        /// </summary>
        /// <param name="up">The movement in the upward direction.</param>
        /// <param name="down">The movement in the downward direction.</param>
        /// <param name="left">The movement in the leftward direction.</param>
        /// <param name="right">The movement in the rightward direction.</param>
        /// <param name="timeSinceLastUpdateInMS">The time elapsed since the last update, in milliseconds.</param>
        public void UpdatePlayerPosition(double up, double down, double left, double right, int timeSinceLastUpdateInMS)
        {
            _player.UpdatePlayerPosition(up, down, left, right, timeSinceLastUpdateInMS);
            
        }

        /// <summary>
        /// Gets the current coordinates of the player within the game world.
        /// </summary>
        /// <returns>A tuple containing the X and Y coordinates of the player.</returns>
        public (int x, int y) GetPlayerCoordinates()
        {
            return (_player.X, _player.Y);
        }

        /// <summary>
        /// Instructs the renderer to draw the terrain for the current level.
        /// </summary>
        /// <param name="renderer">The game renderer responsible for drawing the game objects.</param>
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

        /// <summary>
        /// Retrieves all game objects that can be rendered.
        /// </summary>
        /// <returns>An enumerable collection of renderable game objects.</returns>

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

        /// <summary>
        /// Renders all objects in the game, removing any that are no longer active.
        /// </summary>
        /// <param name="timeSinceLastFrame">The time elapsed since the last frame was rendered, in milliseconds.</param>
        /// <param name="renderer">The renderer used to draw the game objects.</param>
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
        }

        private int _bombIds = 100;
        
        /// <summary>
        /// Adds a bomb game object at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate where the bomb should be placed.</param>
        /// <param name="y">The y-coordinate where the bomb should be placed.</param>
        public void AddBomb(int x, int y)
        {
            AnimatedGameObject bomb = new AnimatedGameObject("BombExploding.png", 2, _bombIds, 13, 13, 1, x, y);
            _gameObjects.Add(bomb.Id, bomb);
            ++_bombIds;
        }
    }
}