using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TheAdventure{
    public unsafe class GameRenderer
    {
        public struct TextureInfo{
            public int Width {get;set;}
            public int Height {get;set;}
            public int PixelDataSize{get { return Width * Height * 4; }}
        }

        Dictionary<int, IntPtr> _texturePointers;
        Dictionary<int, TextureInfo> _textureInformation;
        int _index = 0;

        private Sdl _sdl;
        private Renderer* _renderer;
        private GameWindow _window;
        private GameLogic _gameLogic;

        public GameRenderer(Sdl sdl, GameWindow gameWindow, GameLogic gameLogic){
            _window = gameWindow;
            _gameLogic = gameLogic;
            _sdl = sdl;
            _renderer = (Renderer*)gameWindow.CreateRenderer();
            _textureInformation = new Dictionary<int, TextureInfo>();
            _texturePointers = new Dictionary<int, nint>();
        }

        public void Render(int timeSinceLastFrame){
            _sdl.RenderClear(_renderer);
            foreach(var renderable in _gameLogic.GetRenderables()){
                if(renderable.TextureId > -1 &&
                   _texturePointers.TryGetValue(renderable.TextureId, out var texturePointer)){
                    _sdl.RenderCopyEx(_renderer, (Texture*)texturePointer, renderable.TextureSource, renderable.TextureDestination, 0, new Silk.NET.SDL.Point(0, 0), RendererFlip.None);
                }
            }
            _sdl.RenderPresent(_renderer);
        }

        public int LoadTexture(string fileName, out TextureInfo textureInfo)
        {
            using (var fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)){
                var image = Image.Load<Rgba32>(fStream);
                textureInfo = new TextureInfo() {
                    Width = image.Width,
                    Height = image.Height,
                };
                var imageRAWData = new byte[textureInfo.PixelDataSize];
                image.CopyPixelDataTo(imageRAWData.AsSpan());
                Surface* imageSurface = null;
                Texture* imageTexture = null;
                fixed(byte* data = imageRAWData)
                {
                    imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(data, textureInfo.Width, textureInfo.Height, 8, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
                    imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface); 
                    _sdl.FreeSurface(imageSurface);
                }
                if(imageTexture != null)
                {
                    _texturePointers[_index] = (IntPtr)imageTexture;
                    _textureInformation[_index] = textureInfo;
                    return _index++;
                }
                else{
                    return -1;
                }
            }
        }
    }
}