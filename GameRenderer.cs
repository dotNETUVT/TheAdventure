using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

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
    private int _screenWidth;
    private int _screenHeight;

    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _window = window;
        _sdl = sdl;

        _renderer = (Renderer*)window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        var windowSize = window.Size;
        _camera = new Camera(windowSize.Width, windowSize.Height);

        _screenWidth = window.Size.Width;
        _screenHeight = window.Size.Height; 
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
            var image = SixLabors.ImageSharp.Image.Load<Rgba32>(fStream);
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

    public void RenderPlayerHealth(PlayerObject player)
{
    int healthBarWidth = 200;
    int healthBarHeight = 20;
    int healthBarX = 10;  // 10 pixels from the left
    int healthBarY = _screenHeight - 30;  // 30 pixels from the bottom

    _sdl.SetRenderDrawColor(_renderer, 128, 128, 128, 255);
    var backgroundRect = new Rectangle<int>(healthBarX, healthBarY, healthBarWidth, healthBarHeight);
    _sdl.RenderFillRect(_renderer, &backgroundRect);

    int filledWidth = (int)(healthBarWidth * (player.Health / 100.0));
    _sdl.SetRenderDrawColor(_renderer, 255, 0, 0, 255);
    var filledRect = new Rectangle<int>(healthBarX, healthBarY, filledWidth, healthBarHeight);
    _sdl.RenderFillRect(_renderer, &filledRect);
}

public void RenderPauseOverlay()
{
    // Set a semi-transparent overlay to darken the screen
    _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);
    _sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 128); // Semi-transparent black
    Rectangle<int> fullScreenRect = new Rectangle<int>(0, 0, _screenWidth, _screenHeight);
    _sdl.RenderFillRect(_renderer, ref fullScreenRect);

    // Draw a simple "pause" symbol in the center of the screen
    int pauseWidth = 10;
    int pauseHeight = 50;
    int centerWidth = _screenWidth / 2;
    int centerHeight = _screenHeight / 2;
    int gap = 20;

    // Left part of pause symbol
    Rectangle<int> leftRect = new Rectangle<int>(
        centerWidth - gap - pauseWidth, centerHeight - pauseHeight / 2,
        pauseWidth, pauseHeight
    );
    _sdl.SetRenderDrawColor(_renderer, 255, 255, 255, 255); // White
    _sdl.RenderFillRect(_renderer, ref leftRect);

    // Right part of pause symbol
    Rectangle<int> rightRect = new Rectangle<int>(
        centerWidth + gap, centerHeight - pauseHeight / 2,
        pauseWidth, pauseHeight
    );
    _sdl.RenderFillRect(_renderer, ref rightRect);
}




}