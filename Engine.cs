using TheAdventure.Models;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;

        private readonly MainMenu _mainMenuScene;

        private readonly SimpleWorld _simpleWorldScene;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;
            _mainMenuScene = new MainMenu(renderer, input);
            _simpleWorldScene = new SimpleWorld(renderer, input, _mainMenuScene);
            _mainMenuScene.AddScene(Scenes.simpleWorld, (GameScene)_simpleWorldScene);

            _input.OnMouseClick += (_, coords) => AddBomb(coords.x, coords.y);

            _mainMenuScene.Activate();
        }

        public bool ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            if(_mainMenuScene.ProcessFrame() || _simpleWorldScene.ProcessFrame()){
                return true;
            }


            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }

            return false;
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _mainMenuScene.RenderFrame();
            _simpleWorldScene.RenderFrame();

            RenderAllObjects();

            _renderer.PresentFrame();
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
    }
}
