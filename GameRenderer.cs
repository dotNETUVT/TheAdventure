using System;
using System.Collections.Generic;
using System.IO;
using Silk.NET.Maths;
using Silk.NET.SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

namespace TheAdventure;

public unsafe class GameRenderer : IDisposable
{
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;
    private readonly GameWindow _window;
    private readonly Camera _camera;
    private readonly Dictionary<int, IntPtr> _textures = new();
    private readonly Dictionary<int, TextureInfo> _textureData = new();
    private int _textureId;

    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _sdl = sdl ?? throw new ArgumentNullException(nameof(sdl));

        _renderer = (Renderer*)window.CreateRenderer();
        if (_renderer == null)
        {
            throw new InvalidOperationException("Failed to create SDL renderer.");
        }

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
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

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
                    textureInfo.Height, 32, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);
                if (imageSurface == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to create surface from image.");
                }

                var imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                _sdl.FreeSurface(imageSurface);

                if (imageTexture == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to create texture from surface.");
                }

                _textureData[_textureId] = textureInfo;
                _textures[_textureId] = imageTexture;
            }
        }

        return _textureId++;
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst,
        RendererFlip flip = RendererFlip.None, double angle = 0.0, Point center = default)
    {
        if (_textures.TryGetValue(textureId, out var imageTexture))
        {
            var result = _sdl.RenderCopyEx(_renderer, (Texture*)imageTexture, src,
                _camera.TranslateToScreenCoordinates(dst),
                angle,
                center, flip);
            if (result < 0)
            {
                throw new InvalidOperationException($"RenderCopyEx failed: {_sdl.GetError()}");
            }
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

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var texture in _textures.Values)
            {
                _sdl.DestroyTexture((Texture*)texture);
            }
            _sdl.DestroyRenderer(_renderer);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
