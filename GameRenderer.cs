using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using FontStashSharp;
using FontStashSharp.Interfaces;
using TheAdventure.Models;

using Point = Silk.NET.SDL.Point;

namespace TheAdventure;

public unsafe class GameRenderer : ITexture2DManager, IFontStashRenderer2
{
    private Sdl _sdl;
    private Renderer* _renderer;
    private GameWindow _window;
    private Camera _camera;

    private FontSystem _fontSystem;

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

        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(@"Assets/Minecraft.ttf"));
    }

    public void SetWorldBounds(Rectangle<int> bounds)
    {
        _camera.SetWorldBounds(bounds);
    }

    public void OnWindowSizeChanged()
    {
        _camera.Width = _window.Size.Width;
        _camera.Height = _window.Size.Height;
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
                Height = image.Height,
                Id = _textureId++
            };
            var imageRAWData = new byte[textureInfo.Width * textureInfo.Height * 4];
            image.CopyPixelDataTo(imageRAWData.AsSpan());
            fixed (byte* data = imageRAWData)
            {
                var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(data, textureInfo.Width,
                    textureInfo.Height, 8, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
                var imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                _sdl.FreeSurface(imageSurface);
                _textureData[textureInfo.Id] = textureInfo;
                _textures[textureInfo.Id] = (IntPtr)imageTexture;
            }
        }

        return textureInfo.Id;
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

    public (int Width, int Height) MeasureText(string text, float size)
    {
        var font = _fontSystem.GetFont(size);
        var stringSize = font.MeasureString(text);
        return ((int)stringSize.X, (int)stringSize.Y);
    }

    public void RenderText(string text, float size, int x, int y, int r, int g, int b, bool translateCoordinates = true)
    {
        if (translateCoordinates)
        {
            var worldCoords = TranslateFromScreenToWorldCoordinates(x, y);
            x = worldCoords.X;
            y = worldCoords.Y;
        }

        var font = _fontSystem.GetFont(size);
        font.DrawText(this, text, new Vector2(x, y), new FSColor { R = (byte)r, G = (byte)g, B = (byte)b });
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

    public (int Width, int Height) GetWindowSize()
    {
        return _window.Size;
    }

    #region FontStashSharp interfaces implementation
    public object CreateTexture(int width, int height)
    {
        var t = new TextureInfo() { Width = width, Height = height, Id = _textureId++ };

        var imageTexture = _sdl.CreateTexture(_renderer, (uint)PixelFormatEnum.Rgba32,
            (int)RendererFlags.Accelerated, width, height);

        _sdl.SetTextureBlendMode(imageTexture, BlendMode.Blend);

        _textureData[t.Id] = t;
        _textures[t.Id] = (IntPtr)imageTexture;

        return t;
    }

    public System.Drawing.Point GetTextureSize(object texture)
    {
        var t = (TextureInfo)texture;
        return new System.Drawing.Point(t.Width, t.Height);
    }

    public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
    {
        var t = (TextureInfo)texture;
        if (_textures.TryGetValue(t.Id, out var imageTexture))
        {
            _sdl.SetRenderTarget(_renderer, (Texture*)imageTexture);
            fixed (byte* p_data = data)
            {
                var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(p_data, bounds.Width,
                            bounds.Height, 8, bounds.Width * 4, (uint)PixelFormatEnum.Rgba32);
                var newImageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                _sdl.FreeSurface(imageSurface);

                _sdl.RenderCopyEx(_renderer, newImageTexture, new Rectangle<int>(0, 0, t.Width, t.Height),
                    new Rectangle<int>(bounds.X, bounds.Y, bounds.Width, bounds.Height), 0, null,
                    RendererFlip.None);
                _sdl.DestroyTexture(newImageTexture);
            }
            _sdl.SetRenderTarget(_renderer, null);
        }
    }

    public ITexture2DManager TextureManager => this;

    public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
    {
        var t = (TextureInfo)texture;
        if (_textures.TryGetValue(t.Id, out var imageTexture))
        {
            var sdlTexture = (Texture*)imageTexture;

            _sdl.SetTextureColorMod(sdlTexture, topLeft.Color.R, topLeft.Color.G, topLeft.Color.B);

            var src = new Rectangle<int>((int)(topLeft.TextureCoordinate.X * t.Width),
                (int)(topLeft.TextureCoordinate.Y * t.Height),
                (int)((bottomRight.TextureCoordinate.X - topLeft.TextureCoordinate.X) * t.Width),
                (int)((bottomRight.TextureCoordinate.Y - topLeft.TextureCoordinate.Y) * t.Height));
            var dst = new Rectangle<int>((int)topLeft.Position.X,
                (int)topLeft.Position.Y,
                (int)(bottomRight.Position.X - topLeft.Position.X),
                (int)(bottomRight.Position.Y - topLeft.Position.Y));
            _sdl.RenderCopyEx(_renderer, sdlTexture, src, _camera.TranslateToScreenCoordinates(dst), 0, null, RendererFlip.None);
        }
    }
    #endregion
}
