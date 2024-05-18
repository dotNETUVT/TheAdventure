using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;
using System.Runtime.InteropServices;

namespace TheAdventure;

public unsafe class GameRenderer
{
    private Sdl _sdl;
    private Renderer* _renderer;
    private GameWindow _window;
    private Camera _camera;

    private Dictionary<int, IntPtr> _textures = new();
    private Dictionary<int, TextureInfo> _textureData = new();
    private int _textureId;
    private int _starTextureId;
    private TextureInfo _starTextureInfo;
    private int _numberTextureId;
    private TextureInfo _numberTextureInfo;
    private int _numberHeight = 16;
    private Dictionary<char, int> _numberWidths = new()
    {
        { '0', 14 },
        { '1', 8 },
        { '2', 14 },
        { '3', 14 },
        { '4', 14 },
        { '5', 14 },
        { '6', 14 },
        { '7', 14 },
        { '8', 14 },
        { '9', 14 }
    };
    private Dictionary<char, int> _numberOffsets = new()
    {
        { '0', 0 },
        { '1', 16 },
        { '2', 27 },
        { '3', 47 },
        { '4', 63 },
        { '5', 79 },
        { '6', 95 },
        { '7', 111 },
        { '8', 128 },
        { '9', 144 }
    };

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
    
    public void LoadStarTexture(string fileName)
    {
        _starTextureId = LoadTexture(fileName, out _starTextureInfo);
    }

    public void LoadNumberTexture(string fileName)
    {
        _numberTextureId = LoadTexture(fileName, out _numberTextureInfo);
    }
    
    public void RenderStar(int x, int y, float scale = 1.0f)
    {
        if (_textures.TryGetValue(_starTextureId, out var texture))
        {
            int width = (int)(_starTextureInfo.Width * scale);
            int height = (int)(_starTextureInfo.Height * scale);
            Rectangle<int> srcRect = new Rectangle<int>(0, 0, _starTextureInfo.Width, _starTextureInfo.Height);
            Rectangle<int> dstRect = new Rectangle<int>(x, y, width, height);
            _sdl.RenderCopy(_renderer, (Texture*)texture, &srcRect, &dstRect);
        }
    }
    
    public void RenderNumber(char number, int x, int y, float scale = 1.0f)
    {
        if (_textures.TryGetValue(_numberTextureId, out var texture))
        {
            var width = (int)(_numberWidths[number] * scale);
            var height = (int)(_numberHeight * scale);
            var offsetX = _numberOffsets[number];
            Rectangle<int> srcRect = new Rectangle<int>(offsetX, 0, _numberWidths[number], _numberHeight);
            Rectangle<int> dstRect = new Rectangle<int>(x, y, width, height);
            _sdl.RenderCopy(_renderer, (Texture*)texture, &srcRect, &dstRect);
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Rect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }
    private SDL_Rect ToSDL_Rect(Rectangle<int> rect)
    {
        return new SDL_Rect { x = rect.Origin.X, y = rect.Origin.Y, w = rect.Size.X, h = rect.Size.Y };
    }
}