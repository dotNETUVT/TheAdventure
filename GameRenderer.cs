using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

namespace TheAdventure
{
    public unsafe class GameRenderer
    {
        private Sdl _sdl;
        private Renderer* _renderer;
        private GameWindow _window;
        private Camera _camera;

        private Dictionary<int, IntPtr> _textures = new();
        private Dictionary<int, TextureInfo> _textureData = new();
        private List<LightSource> _lightSources = new();
        private int _textureId;

        public GameRenderer(Sdl sdl, GameWindow window)
        {
            _window = window;
            _sdl = sdl;

            _renderer = (Renderer*)window.CreateRenderer();
            _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

            var windowSize = window.Size;
            _camera = new Camera(windowSize.Width, windowSize.Height);
        }

        public void SetWorldBounds(Rectangle<int> bounds)
        {
            _camera.SetWorldBounds(bounds);
        }

        public void CameraLookAt(int x, int y)
        {
            _camera.LookAt(x, y);
        }

        public int LoadTexture(string fileName, out TextureInfo textureInfo)
        {
            using (var fStream = new FileStream(fileName, FileMode.Open))
            {
                var image = Image.Load<Rgba32>(fStream);
                textureInfo = new TextureInfo()
                {
                    Width = image.Width,
                    Height = image.Height
                };
                var imageRAWData = new byte[textureInfo.Width * textureInfo.Height * 4];
                image.CopyPixelDataTo(imageRAWData.AsSpan());
                fixed (byte* data = imageRAWData)
                {
                    var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(data, textureInfo.Width,
                        textureInfo.Height, 8, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
                    var imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                    _sdl.FreeSurface(imageSurface);
                    _textureData[_textureId] = textureInfo;
                    _textures[_textureId] = (IntPtr)imageTexture;
                }
            }

            return _textureId++;
        }

        public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst,
            RendererFlip flip = RendererFlip.None, double angle = 0.0, Point center = default)
        {
            if (_textures.TryGetValue(textureId, out var imageTexture))
            {
                _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, src,
                    _camera.TranslateToScreenCoordinates(dst),
                    angle,
                    center, flip);
            }
        }

        public Vector2D<int> TranslateFromScreenToWorldCoordinates(int x, int y)
        {
            return _camera.FromScreenToWorld(x, y);
        }

        public void SetDrawColor(byte r, byte g, byte b, byte a)
        {
            _sdl.SetRenderDrawColor(_renderer, r, g, b, a);
        }

        public void ClearScreen()
        {
            _sdl.RenderClear(_renderer);
        }

        public void PresentFrame()
        {
            _sdl.RenderPresent(_renderer);
        }

        public void AddLightSource(LightSource lightSource)
        {
            _lightSources.Add(lightSource);
        }

        public void RenderLighting()
        {
            foreach (var light in _lightSources)
            {
                var lightTexture = _sdl.CreateTexture(_renderer, (uint)PixelFormatEnum.Rgba32, (int)TextureAccess.Target, light.Radius * 2, light.Radius * 2);
                _sdl.SetRenderTarget(_renderer, lightTexture);
                _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);
                _sdl.SetRenderDrawColor(_renderer, light.Color.R, light.Color.G, light.Color.B, light.Color.A);
                RenderFillCircle(light.Radius, light.Radius, light.Radius); 
                _sdl.SetRenderTarget(_renderer, null);
                var dst = new Rectangle<int>(light.Position.X - light.Radius, light.Position.Y - light.Radius, light.Radius * 2, light.Radius * 2);
                _sdl.RenderCopy(_renderer, lightTexture, null, _camera.TranslateToScreenCoordinates(dst));
                _sdl.DestroyTexture(lightTexture);
            }
        }
        public IEnumerable<LightSource> GetLightSources()
        {
            return _lightSources;
        }

        private void RenderFillCircle(int centerX, int centerY, int radius)
        {
            for (int w = 0; w < radius * 2; w++)
            {
                for (int h = 0; h < radius * 2; h++)
                {
                    int dx = radius - w;
                    int dy = radius - h;
                    if ((dx * dx + dy * dy) <= (radius * radius))
                    {
                        _sdl.RenderDrawPoint(_renderer, centerX + dx, centerY + dy);
                    }
                }
            }
        }
    }
}
