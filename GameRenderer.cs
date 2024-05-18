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

    public int Width => _window.Size.Width;
    public int Height => _window.Size.Height;

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

    public void RenderGameOverMessage()
    {
        // Render a white rectangle in the center of the screen
        SetDrawColor(255, 255, 255, 255); // White background
        Rectangle<int> rect = new Rectangle<int>((Width / 2) - 100, (Height / 2) - 25, 200, 50);
        _sdl.RenderFillRect(_renderer, rect);

        // Render black text "Game Over!" (rudimentary example)
        SetDrawColor(0, 0, 0, 255); // Black text
        DrawText("Game Over!", (Width / 2) - 80, (Height / 2) - 15);
    }

    private void DrawText(string text, int x, int y)
    {
        // Simple implementation of text rendering using rectangles
        foreach (char c in text)
        {
            DrawChar(c, ref x, y);
            x += 12; // Space between characters
        }
    }

    private void DrawChar(char c, ref int x, int y)
    {
        switch (c)
        {
            case 'G':
                _sdl.RenderDrawLine(_renderer, x, y, x + 10, y);
                _sdl.RenderDrawLine(_renderer, x, y, x, y + 20);
                _sdl.RenderDrawLine(_renderer, x, y + 20, x + 10, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 10, y + 20, x + 10, y + 10);
                _sdl.RenderDrawLine(_renderer, x + 5, y + 10, x + 10, y + 10);
                break;
            case 'a':
                _sdl.RenderDrawLine(_renderer, x + 2, y + 10, x + 8, y + 10);
                _sdl.RenderDrawLine(_renderer, x + 2, y + 10, x + 2, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 8, y + 10, x + 8, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 2, y + 15, x + 8, y + 15);
                break;
            case 'm':
                _sdl.RenderDrawLine(_renderer, x, y + 20, x, y + 10);
                _sdl.RenderDrawLine(_renderer, x, y + 10, x + 5, y + 15);
                _sdl.RenderDrawLine(_renderer, x + 5, y + 15, x + 10, y + 10);
                _sdl.RenderDrawLine(_renderer, x + 10, y + 10, x + 10, y + 20);
                break;
            case 'e':
                _sdl.RenderDrawLine(_renderer, x, y + 10, x + 10, y + 10);
                _sdl.RenderDrawLine(_renderer, x, y + 10, x, y + 20);
                _sdl.RenderDrawLine(_renderer, x, y + 15, x + 7, y + 15);
                _sdl.RenderDrawLine(_renderer, x, y + 20, x + 10, y + 20);
                break;
            case 'O':
                _sdl.RenderDrawLine(_renderer, x, y, x + 10, y);
                _sdl.RenderDrawLine(_renderer, x, y, x, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 10, y, x + 10, y + 20);
                _sdl.RenderDrawLine(_renderer, x, y + 20, x + 10, y + 20);
                break;
            case 'v':
                _sdl.RenderDrawLine(_renderer, x, y + 10, x + 5, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 5, y + 20, x + 10, y + 10);
                break;
            case 'r':
                _sdl.RenderDrawLine(_renderer, x + 2, y + 10, x + 8, y + 10);
                _sdl.RenderDrawLine(_renderer, x + 2, y + 10, x + 2, y + 20);
                _sdl.RenderDrawLine(_renderer, x + 8, y + 10, x + 8, y + 15);
                _sdl.RenderDrawLine(_renderer, x + 2, y + 15, x + 8, y + 15);
                _sdl.RenderDrawLine(_renderer, x + 2, y + 15, x + 8, y + 20); // Diagonal line for 'R'
                break;
            case '!':
                _sdl.RenderDrawLine(_renderer, x + 5, y, x + 5, y + 15);
                _sdl.RenderDrawLine(_renderer, x + 5, y + 17, x + 5, y + 20);
                break;
        }
    }
}
