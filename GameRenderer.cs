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

    public void LoadMainMenuTileSet(MainMenuTileSet tileSet)
    {
        var fileName = Path.Combine("Assets", "mainMenu", tileSet.Image);
        tileSet.Tiles = new Tile[tileSet.TileCount];
        using (var fStream = new FileStream(fileName, FileMode.Open))
        {
            var image = Image.Load<Rgba32>(fStream);
            var textureInfo = new TextureInfo()
            {
                Width = image.Width,
                Height = image.Height
            };
            int numberOfRows = image.Height / tileSet.TileHeight;
            int numberOfColumns = image.Width / tileSet.TileWidth;
            int firstgid = tileSet.Firstgid;
            int tilesCount = 0;
            for(int i = 0; i < numberOfRows; i++)
            {
                for(int j = 0; j < numberOfColumns; j++)
                {
                    byte[] byteArray = new byte[tileSet.TileWidth * tileSet.TileHeight * 4];

                    int byteIndex = 0;
                    for (int y = i * tileSet.TileHeight; y < (i + 1) * tileSet.TileHeight; y++)
                    {
                        for (int x = j * tileSet.TileWidth; x < (j + 1) * tileSet.TileWidth; x++)
                        {
                            Rgba32 pixel = image[x, y];

                            byteArray[byteIndex++] = pixel.R;
                            byteArray[byteIndex++] = pixel.G;
                            byteArray[byteIndex++] = pixel.B;
                            byteArray[byteIndex++] = pixel.A;
                        }
                    }

                    fixed (byte* data = byteArray)
                    {
                        var imageSurface = _sdl.CreateRGBSurfaceWithFormatFrom(data, tileSet.TileWidth,
                            tileSet.TileHeight, 8, tileSet.TileWidth * 4, (uint)PixelFormatEnum.Rgba32);
                        var imageTexture = _sdl.CreateTextureFromSurface(_renderer, imageSurface);
                        _sdl.FreeSurface(imageSurface);
                        _textureData[_textureId] = textureInfo;
                        _textures[_textureId] = (IntPtr)imageTexture;
                    }

                    _textureId++;
                    tileSet.Tiles[tilesCount++] = new Tile()
                    {
                        Id = firstgid++,
                        Image = fileName,
                        ImageWidth = tileSet.TileWidth,
                        ImageHeight = tileSet.TileHeight,
                        InternalTextureId = _textureId
                    };
                }
            }
        }
    }

    public void UnloadTexture(int textureId)
    {
        if (_textures.TryGetValue(textureId, out var imageTexture))
        {
            _sdl.DestroyTexture((Texture*)imageTexture);
            _textures.Remove(textureId);
            _textureData.Remove(textureId);
        }
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
}