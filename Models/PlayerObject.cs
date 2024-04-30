using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";
    private int posX {  get; set; }
    private int posY { get; set; }


    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
        posX = x;
        posY = y;
    }

    public string getCurrentAnimation()
    {
        return _currentAnimation;
    }

    public void setPosition(int x, int y)
    {
        posX += x;
        posY += y;

        Position = (posX, posY);
    }

    public bool collide(int object_x, int object_y, int object_width, int object_height)
    {
        if (posX + 8 >= object_x - object_width / 2 && posX - 8 <= object_x + object_width / 2 &&
            posY >= object_y - object_height / 2 && posY - 22 <= object_y - object_height / 2)
            return true;
        return false;
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
        posX = x;
        posY = y;
    }
}