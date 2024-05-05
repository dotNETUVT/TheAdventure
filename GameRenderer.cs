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
            textureInfo = new TextureInfo
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

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst, RendererFlip flip = RendererFlip.None, double angle = 0.0, Point? center = null)
    {
        if (_textures.TryGetValue(textureId, out var imageTexture))
        {
            _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, src,
                _camera.TranslateToScreenCoordinates(dst),
                angle,
                center ?? default(Point), flip);
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

    public void FillRect(Rectangle<int> rect)
    {
        _sdl.RenderFillRect(_renderer, rect);
    }

    public int Width => _window.Size.Width;
    public int Height => _window.Size.Height;

    public void RenderRedFlash()
    {
        SetDrawColor(255, 0, 0, 128); 
        FillRect(new Rectangle<int>(0, 0, 100, 100));
        FillRect(new Rectangle<int>(Width - 100, 0, 100, 100));
        FillRect(new Rectangle<int>(0, Height - 100, 100, 100));
        FillRect(new Rectangle<int>(Width - 100, Height - 100, 100, 100));
    }
    
   public void FlashWhiteEffect(int durationMs)
{
    // Calculate the number of frames and the opacity decrement per frame
    int numFrames = durationMs / 10; // Assuming 100 frames per second
    double opacityDecrement = 255.0 / numFrames;

    // Perform the flashing effect
    Task.Run(async () =>
    {
        for (int i = 0; i < numFrames; i++)
        {
            // Calculate the current opacity
            byte opacity = (byte)(255.0 - i * opacityDecrement);

            // Set the draw color with the current opacity
            SetDrawColor(255, 255, 255, opacity);

            // Fill the entire screen with white
            FillRect(new Rectangle<int>(0, 0, Width, Height));

            // Render the frame
            PresentFrame();

            // Wait for a short duration
        }
    });
}


}


