using System.Runtime.CompilerServices;
using Silk.NET.Maths;
using TheAdventure.Models;

namespace TheAdventure
{
    public class GameLogic
    {
        public GameLogic()
        {
            _gameObjects = new List<GameObject>();
        }

        public void InitializeGame(GameRenderer gameRenderer)
        {
            _gameRenderer = gameRenderer;
        }
        
        public void AddGameObject(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
        }
        
        int frameCount = 0;

        public void ProcessFrame(int timeSinceLastFrame)
        {
            var destroyedObjects = new List<GameObject>();
            foreach (var gameObject in _gameObjects)
            {
                if (!gameObject.Update(timeSinceLastFrame))
                {
                    destroyedObjects.Add(gameObject);
                }
            }
            
            foreach (var destroyedObject in destroyedObjects)
            {
                _gameObjects.Remove(destroyedObject);
            }
        }

        private GameRenderer _gameRenderer;
        private List<GameObject> _gameObjects;

        public IEnumerable<RenderableGameObject> GetRenderables()
        {
            if (_gameObjects.Count == 0)
            {
                yield break;
            }
            
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject is RenderableGameObject)
                {
                    yield return (RenderableGameObject)gameObject;
                }
            }
        }
    }
}