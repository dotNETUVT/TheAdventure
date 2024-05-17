using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

namespace TheAdventure;

public unsafe class GameRenderer
{
    public Sdl _sdl;
    public Renderer* _renderer;
    private GameWindow _window;
    private Camera _camera;

    private Dictionary<int, IntPtr> _textures = new();
    private Dictionary<int, TextureInfo> _textureData = new();
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
                    textureInfo.Height, 32, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
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

    public void RenderSuperPowerIcon(int textureId, SuperPower superPower, int x, int y, int width, int height)
    {
        // Render the icon without using the camera to translate coordinates
        if (_textures.TryGetValue(textureId, out var imageTexture))
        {
            var srcRect = new Rectangle<int>(0, 0, 512, 512);
            var dstRect = new Rectangle<int>(x, y, width, height);
            _sdl.RenderCopy(_renderer, (Texture*)imageTexture, &srcRect, &dstRect);
        }

        // Render cooldown timer
        var cooldownTime = superPower.GetCooldownTimeRemaining();
        if (cooldownTime > 0)
        {
            var cooldownHeight = (int)(height * ((float)cooldownTime / SuperPower.CooldownTime));
            SetDrawColor(255, 255, 255, 128); // Semi-transparent white
            var cooldownRect = new Rectangle<int>(x, y + height - cooldownHeight, width, cooldownHeight);
            _sdl.RenderFillRect(_renderer, &cooldownRect);
        }
    }
}