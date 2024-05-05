using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";

    private int _hearthTextureId; // texture id

    public int _health { get; set; } = 3; // healt property for the player

    private GameRenderer _renderer;



    public PlayerObject(GameRenderer renderer, SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
        
        _renderer = renderer;
        TextureInfo textureInfo;

        _hearthTextureId = _renderer.LoadTexture("Assets/life_resized.png", out textureInfo);  // load texture


       
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time)
    {

        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            _currentAnimation == "IdleDown"){
            return;
        }

        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        if (x < 10)
        {
            x = 10;
        }

        if (y < 24)
        {
            y = 24;
        }

        if (x > width - 10)
        {
            x = width - 10;
        }

        if (y > height - 6)
        {
            y = height - 6;
        }

        if (y < Position.Y && _currentAnimation != "MoveUp"){
            _currentAnimation = "MoveUp";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (y > Position.Y && _currentAnimation != "MoveDown"){
            _currentAnimation = "MoveDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x > Position.X && _currentAnimation != "MoveRight"){
            _currentAnimation = "MoveRight";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x < Position.X && _currentAnimation != "MoveLeft"){
            _currentAnimation = "MoveLeft";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x == Position.X && _currentAnimation != "IdleDown" &&
            y == Position.Y && _currentAnimation != "IdleDown"){
            _currentAnimation = "IdleDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }

        //Console.WriteLine($"Will to switch to {_currentAnimation}");
        SpriteSheet.ActivateAnimation(_currentAnimation);
        Position = (x, y);
    }

// method for rendering a player's life
    public void RenderHearths(GameRenderer render)
    {
        int heartWidth = 32;
        int heartHeight = 32;
        int margin = 10;

        for (int i = 0; i < _health; i++)
        {   
            int screenX = margin + i * (heartWidth + 5);
            int screenY = margin;

            render.RenderTextureToScreen(_hearthTextureId, new Rectangle<int>(0, 0, 32, 32), new Rectangle<int>(screenX, screenY, heartWidth, heartHeight));

        }
    }
}