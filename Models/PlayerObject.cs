using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";


    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
       
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time, IEnumerable<Hitbox> hitboxes)
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

        foreach (var hitbox in hitboxes)
            if (CollidesWithHitbox(x, y, hitbox)) return;

        Position = (x, y);
    }

    private bool CollidesWithHitbox(int x, int y, Hitbox hitbox)
    {
        int playerRight = x + SpriteSheet.FrameWidth / 12;
        int playerTop = y - 18; 

        int hitboxRight = hitbox.x + hitbox.Width;
        int hitboxBottom = hitbox.y + hitbox.Height;

        // Collision occurs if any of the player's edges overlap with the hitbox boundaries
        return x - SpriteSheet.FrameWidth / 12 < hitboxRight &&
               playerRight > hitbox.x &&
               playerTop < hitboxBottom &&
               y > hitbox.y;
    }
}