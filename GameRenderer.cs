using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TheAdventure
{
    /// <summary>
    /// Responsible for rendering game elements to the screen.
    /// It utilizes Silk.NET for rendering and ImageSharp for texture management.
    /// </summary>
    public unsafe class GameRenderer
    {
        /// <summary>
        /// Holds information about a texture, including its dimensions and pixel data size.
        /// </summary>
        public struct TextureInfo
        {
            /// <value>Texture width in pixels.</value>
            public int Width { get; set; }
            /// <value>Texture height in pixels.</value>
            public int Height { get; set; }

            /// <summary>
            /// Calculates the total size of the pixel data in bytes.
            /// Assumes a 32-bit color depth (4 bytes per pixel).
            /// </summary>
            public int PixelDataSize
            {
                get { return Width * Height * 4; }
            }
        }

        private Sdl _sdl;
        private Renderer* _renderer;
        private GameWindow _window;
        private GameLogic _gameLogic;
        private GameCamera _camera;

        private Dictionary<int, IntPtr> _textures;
        private Dictionary<int, TextureData> _textureData;
        private int _textureId;

        private static GameRenderer? _singleton;
        private DateTimeOffset _lastFrameRenderedAt = DateTimeOffset.MinValue;

        /// <summary>
        /// Initializes a new instance of the GameRenderer class.
        /// </summary>
        /// <param name="sdl">The SDL context for rendering operations.</param>
        /// <param name="gameWindow">The window where the game is displayed.</param>
        /// <param name="gameLogic">The game logic instance for accessing game state.</param>
        public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic)
        {
            _window = gameWindow;
            _gameLogic = gameLogic;
            _sdl = sdl;
            _renderer = (Renderer*)gameWindow.CreateRenderer();
            _textures = new Dictionary<int, IntPtr>();
            _textureData = new Dictionary<int, TextureData>();
            _camera = new GameCamera();
            _camera.Width = 800;
            _camera.Height = 600;

            // TODO: Check if _singleton is not null, if it is, clear resources.

            _singleton = this;
        }

        /// <summary>
        /// Loads a texture from a file and stores it for rendering.
        /// </summary>
        /// <param name="fileName">The file path of the texture to load.</param>
        /// <param name="textureData">Outputs the loaded texture data including dimensions.</param>
        /// <returns>The ID of the loaded texture.</returns>
        public static int LoadTexture(string fileName, out TextureData textureData)
        {
            using (var fStream = new FileStream(fileName, FileMode.Open))
            {
                var image = Image.Load<Rgba32>(fStream);
                textureData = new TextureData()
                {
                    Width = image.Width,
                    Height = image.Height
                };
                var imageRAWData = new byte[textureData.Width * textureData.Height * 4];
                image.CopyPixelDataTo(imageRAWData.AsSpan());
                fixed (byte* data = imageRAWData)
                {
                    if (_singleton == null)
                    {
                        throw new InvalidOperationException("GameRenderer singleton is not initialized.");
                    }
                    
                    var imageSurface = _singleton._sdl.CreateRGBSurfaceWithFormatFrom(data, textureData.Width,
                        textureData.Height, 8, textureData.Width * 4, (uint)PixelFormatEnum.Rgba32);
                    var imageTexture = _singleton._sdl.CreateTextureFromSurface(_singleton._renderer, imageSurface);
                    _singleton._sdl.FreeSurface(imageSurface);
                    _singleton._textureData[_singleton._textureId] = textureData;
                    _singleton._textures[_singleton._textureId] = (IntPtr)imageTexture;
                }
            }

            return _singleton._textureId++;
        }

        /// <summary>
        /// Renders a game object to the screen using its associated texture.
        /// </summary>
        /// <param name="gameObject">The game object to render.</param>
        public void RenderGameObject(RenderableGameObject gameObject)
        {
            // Translate to screen coordinates using camera data.

            if (_textures.TryGetValue(gameObject.TextureId, out var imageTexture))
            {
                _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, gameObject.TextureSource,
                    _camera.TranslateToScreenCoordinates(gameObject.TextureDestination),
                    gameObject.TextureRotation,
                    gameObject.TextureRotationCenter, RendererFlip.None);
            }
        }

        /// <summary>
        /// Renders a texture to the screen at specified source and destination rectangles.
        /// </summary>
        /// <param name="textureId">The ID of the texture to render.</param>
        /// <param name="src">The source rectangle within the texture to render.</param>
        /// <param name="dst">The destination rectangle on the screen where the texture will be rendered.</param>
        public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst)
        {
            // Translate to screen coordinates using camera data.

            if (_textures.TryGetValue(textureId, out var imageTexture))
            {
                _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, src,
                    _camera.TranslateToScreenCoordinates(dst),
                    0,
                    new Silk.NET.SDL.Point(0,0), RendererFlip.None);
            }
        }

        /// <summary>
        /// Executes the rendering process for the current frame. This method should be called every game loop iteration.
        /// </summary>
        public void Render()
        {
            var playerPos = _gameLogic.GetPlayerCoordinates();

            // TODO: implement the soft margin;

            _camera.X = playerPos.x;
            _camera.Y = playerPos.y;

            _sdl.RenderClear(_renderer);

            _gameLogic.RenderTerrain(this);

            var timeSinceLastFrame = 0;
            if (_lastFrameRenderedAt > DateTimeOffset.MinValue)
            {
                timeSinceLastFrame = (int)DateTimeOffset.UtcNow.Subtract(_lastFrameRenderedAt).TotalMilliseconds;
            }

            _gameLogic.RenderAllObjects(timeSinceLastFrame, this);

            _lastFrameRenderedAt = DateTimeOffset.UtcNow;

            _sdl.RenderPresent(_renderer);
        }
    }
}