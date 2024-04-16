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
    private Camera _camera; // doar el cunoste camera

    private Dictionary<int, IntPtr> _textures = new();
    private Dictionary<int, TextureInfo> _textureData = new();
    private int _textureId;

    private List<PlayerObject> _players = new List<PlayerObject>();

    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _window = window;
        _sdl = sdl;

        _renderer = (Renderer*)window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        var windowSize = window.Size;
        _camera = new Camera(windowSize.Width, windowSize.Height);
    }

    public void AddPlayer(PlayerObject player)
    {
        _players.Add(player);
    }

    // magrinile lumii
    public void SetWorldBounds(Rectangle<int> bounds)
    {
        _camera.SetWorldBounds(bounds);
    }

    // puntul in care se uita camera
    public void CameraLookAt(int x1, int y1, int x2, int y2)
    {
        int result = _camera.LookAt(x1, y1, x2, y2);
        if (result == 11)
        {
            // player1 - nu se mai poate misca in jos
            // player2 - nu se mai poate misca in sus
            _players[0].canMoveDown = false;
            _players[1].canMoveUp = false;
        }
        else if (result == 12)
        {
            _players[1].canMoveDown = false;
            _players[0].canMoveUp = false;
        }
        else if (result == 21)
        {
            // player1 - nu se mai poate misca in stanga
            // player2 - nu se mai poate misca in dreapta
            _players[0].canMoveLeft = false;
            _players[1].canMoveRight = false;
        }
        else if (result == 22)
        {
            _players[1].canMoveLeft = false;
            _players[0].canMoveRight = false;
        }
        else
        {
            // pot merge oricum
            foreach (PlayerObject player in _players)
            {
                player.canMoveUp = true;
                player.canMoveDown = true;
                player.canMoveLeft = true;
                player.canMoveRight = true;
            }
        }
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

    // flip - pentru a anima caracterul sa mearga in stanga
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