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
        double time, IEnumerable<TemporaryGameObject> gameObject)
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

        foreach( var obj in gameObject )
        {
            // Calling function for collision, still needs work
            CollidesWithBomb(x, y, obj);
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

    // Function to prevent the player colliding with the bomb, stll needs improvements
    private bool CollidesWithBomb(int x, int y, TemporaryGameObject bomb)
    {
        int playerRight = x + 4; ;

        int bombRight = bomb.Position.X + 16;
        int bombBottom = bomb.Position.Y + 48;

        return x - 4 < bombRight &&
               playerRight > bomb.Position.X &&
               y < bombBottom &&
               y > bomb.Position.Y;
    }

    // Method to get the bounding box of the player
    public System.Drawing.Rectangle GetBoundingBox()
    {
        // Assuming player's bounding box is a rectangle centered at its position with width and height of sprite
        return new System.Drawing.Rectangle((int)(Position.X - SpriteSheet.FrameWidth / 2), (int)(Position.Y - SpriteSheet.FrameHeight / 2), SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
    }
}