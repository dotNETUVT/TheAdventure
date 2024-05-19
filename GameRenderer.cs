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
    
    private int _skyTextureId = -1;
    private TextureInfo _skyTextureInfo;
    
    private Level? _currentLevel;
    
    private bool _cinematicCircleClosing = false;
    private bool _cinematicCircleOpening = false;
    private DateTimeOffset _cinematicStartTime;
    private readonly TimeSpan _cinematicDuration = TimeSpan.FromSeconds(1);
    private bool _cinematicCircleClosed = false;

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

    public void LoadSkyTexture(string fileName)
    {
        _skyTextureId = LoadTexture(fileName, out _skyTextureInfo);
    }

    public void RenderSky(int height)
    {
        if (_skyTextureId != -1)
        {
            var src = new Rectangle<int>(0, 0, _skyTextureInfo.Width, _skyTextureInfo.Height);
            var dst = new Rectangle<int>(0, 0, _window.Size.Width, height);
            _sdl.RenderCopy(_renderer, (Texture*)_textures[_skyTextureId], src, dst);
        }
    }
    
    public void SetCurrentLevel(Level level)
    {
        _currentLevel = level;
    }
    
    public void RenderTerrainShifted(int shiftY)
    {
        if (_currentLevel == null) return;
        for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
        {
            var cLayer = _currentLevel.Layers[layer];

            for (var i = 0; i < _currentLevel.Width; ++i)
            {
                for (var j = 0; j < _currentLevel.Height; ++j)
                {
                    var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                    var cTile = GetTile(cTileId);
                    if (cTile == null) continue;

                    var src = new Rectangle<int>(0, 0, cTile.ImageWidth, cTile.ImageHeight);
                    var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight + shiftY, cTile.ImageWidth, cTile.ImageHeight);

                    RenderTexture(cTile.InternalTextureId, src, dst);
                }
            }
        }
    }

    private Tile? GetTile(int id)
    {
        if (_currentLevel == null) return null;
        foreach (var tileSet in _currentLevel.TileSets)
        {
            foreach (var tile in tileSet.Set.Tiles)
            {
                if (tile.Id == id)
                {
                    return tile;
                }
            }
        }

        return null;
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
    public void StartCinematicCircleClosing()
    {
        _cinematicCircleClosing = true;
        _cinematicStartTime = DateTimeOffset.Now;
        _cinematicCircleClosed = false;
        Console.WriteLine("Starting cinematic circle closing.");
    }

    public void StartCinematicCircleOpening()
    {
        _cinematicCircleOpening = true;
        _cinematicStartTime = DateTimeOffset.Now;
        Console.WriteLine("Starting cinematic circle opening.");
    }

    public bool IsCinematicCircleClosed()
    {
        return _cinematicCircleClosed;
    }

    public void RenderCinematicCircle()
    {
        if (_cinematicCircleClosing || _cinematicCircleOpening)
        {
            var elapsed = (DateTimeOffset.Now - _cinematicStartTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / _cinematicDuration.TotalMilliseconds, 1.0);
            if (_cinematicCircleOpening)
            {
                progress = 1.0 - progress;
            }

            int width = _window.Size.Width;
            int height = _window.Size.Height;
            int centerX = width / 2;
            int centerY = height / 2;
            int maxRadius = (int)Math.Sqrt(centerX * centerX + centerY * centerY);
            int currentRadius = (int)(maxRadius * progress);

            _sdl.SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            _sdl.RenderDrawCircle(_renderer, centerX, centerY, currentRadius);

            if (_cinematicCircleClosing && progress >= 1.0)
            {
                _cinematicCircleClosed = true;
                _cinematicCircleClosing = false;
                Console.WriteLine("Cinematic circle closed.");
            }
            else if (_cinematicCircleOpening && progress <= 0.0)
            {
                _cinematicCircleOpening = false;
                _cinematicCircleClosed = false;
                Console.WriteLine("Cinematic circle opened.");
            }
        }
    }
}

public static unsafe class SdlRendererExtensions
{
    public static void RenderDrawCircle(this Sdl sdl, Renderer* renderer, int centerX, int centerY, int radius)
    {
        for (int w = 0; w < radius * 2; w++)
        {
            for (int h = 0; h < radius * 2; h++)
            {
                int dx = radius - w;
                int dy = radius - h;
                if ((dx * dx + dy * dy) <= (radius * radius))
                {
                    sdl.RenderDrawPoint(renderer, centerX + dx, centerY + dy);
                    sdl.RenderDrawPoint(renderer, centerX - dx, centerY + dy);
                    sdl.RenderDrawPoint(renderer, centerX + dx, centerY - dy);
                    sdl.RenderDrawPoint(renderer, centerX - dx, centerY - dy);
                }
            }
        }
    }
}