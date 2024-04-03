using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TheAdventure
{
    public unsafe class GameRenderer
    {
        public struct TextureInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public int PixelDataSize
            {
                get { return Width * Height * 4; }
            }
        }

        private Sdl _sdl;
        private Renderer* _renderer;
        private GameWindow _window;
        private GameLogic _gameLogic;

        private Dictionary<int, IntPtr> _textures;
        private Dictionary<int, TextureData> _textureData;
        private int _textureId;

        private static GameRenderer? _singleton;
        private DateTimeOffset _lastFrameRenderedAt = DateTimeOffset.MinValue;

        public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic)
        {
            _window = gameWindow;
            _gameLogic = gameLogic;
            _sdl = sdl;
            _renderer = (Renderer*)gameWindow.CreateRenderer();
            _textures = new Dictionary<int, IntPtr>();
            _textureData = new Dictionary<int, TextureData>();

            // TODO: Check if _singleton is not null, if it is, clear resources.

            _singleton = this;
        }

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

        public void RenderGameObject(RenderableGameObject gameObject)
        {
            if (_textures.TryGetValue(gameObject.TextureId, out var imageTexture))
            {
                //_sdl.SetTextureAlphaMod(gameObject.TextureId, 155);
                _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, gameObject.TextureSource,
                    gameObject.TextureDestination,
                    gameObject.TextureRotation,
                    gameObject.TextureRotationCenter, RendererFlip.None);
            }
        }

        public void RenderTexture(int textureId, float textureOpacity, Rectangle<int> src, Rectangle<int> dst)
        {
            if (_textures.TryGetValue(textureId, out var imageTexture))
            {
                byte opacity255 = (byte)(textureOpacity * 255);
                _sdl.SetTextureAlphaMod((Texture*)imageTexture, opacity255);
                _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, src,
                    dst,
                    0,
                    new Silk.NET.SDL.Point(0,0), RendererFlip.None);
            }
        }

        public void Render()
        {
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