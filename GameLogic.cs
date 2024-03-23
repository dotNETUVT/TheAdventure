using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace TheAdventure
{
    public class GameLogic
    {
        private static Dictionary<int, GameObject> _gameObjects = new();
        // Instantiate a new event listener
        private GameEventListener _gameEventListener = new GameEventListener(_gameObjects);
        public GameLogic()
        {
        }

        public void LoadGameState()
        {
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
            // Continuously listen for new events and handle them
            _gameEventListener.Listen();
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
        }

        private int _bombIds = 100;

        public void AddBomb(int x, int y)
        {
            // Dispatch an event
            // The name of the attributes are exactly the same everywhere to avoid confusions.
            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                {"fileName", "BombExploding.png"},
                {"durationInSeconds", "2"},
                {"numberOfFrames", "13"},
                {"numberOfColumns", "13"},
                {"numberOfRows", "1"},
                {"x", x.ToString()},
                {"y", y.ToString()},
            };
            
            _gameEventListener.DispatchEvent(GameEventTypes.spawnAnimatedGameObject, settings, _bombIds);
            ++_bombIds;
        }
    }
}