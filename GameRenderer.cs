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

    private IntPtr _pauseButtonTexture;
    private bool _showPauseButton = false;

    private int _frameCount = 0;
    private double _lastFpsUpdateTime = 0;
    private double _fps = 0;
    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _window = window;
        _sdl = sdl;

        _renderer = (Renderer*)window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        var windowSize = window.Size;
        _camera = new Camera(windowSize.Width, windowSize.Height);

        _lastFpsUpdateTime = _sdl.GetTicks();

        LoadPauseButton("Assets/pausebtn.png");
    }

    private void LoadPauseButton(string filePath)
    {
        TextureInfo textureInfo;
        _pauseButtonTexture = LoadTexture(filePath, out textureInfo);
    }

    public void TogglePauseButtonDisplay(bool show)
    {
        _showPauseButton = show;
    }

    public struct SDL_Color {
    public byte r, g, b, a;
    }

    public void Update(double deltaTime)
    {
        _frameCount++;
        if (_sdl.GetTicks() - _lastFpsUpdateTime > 1000) // Update every second
        {
            _fps = _frameCount / ((_sdl.GetTicks() - _lastFpsUpdateTime) / 1000.0);
            _frameCount = 0;
            _lastFpsUpdateTime = _sdl.GetTicks();
        }
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
        _sdl.Delay(16); // Cap the frame rate to 60 FPS.

        if (_showPauseButton)
        {
            var pauseButtonRect = new Rectangle<int>(0, 0, 64, 64);
            RenderTexture((int)_pauseButtonTexture, pauseButtonRect, pauseButtonRect); // Explicitly cast _pauseButtonTexture to int
        }

        //FPS counter
        SDL_Color fpsColor = new SDL_Color { r = 255, g = 255, b = 255, a = 255 }; // White

        
        
    }
}