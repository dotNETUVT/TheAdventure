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

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
    {
        // Calculate total movement for this frame
        var deltaX = (right - left) * time * _pixelsPerSecond;
        var deltaY = (down - up) * time * _pixelsPerSecond;

        // Update player's position
        var x = Position.X + (int)deltaX;
        var y = Position.Y - (int)deltaY;

        // Clamp the position to the screen boundaries
        x = Math.Clamp(x, 10, width - 10);
        y = Math.Clamp(y, 24, height - 6);

        // Set the new position
        Position = (x, y);

        // Update animation based on movement direction
        if (deltaY < 0 && _currentAnimation != "MoveUp")
        {
            _currentAnimation = "MoveUp";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
        else if (deltaY > 0 && _currentAnimation != "MoveDown")
        {
            _currentAnimation = "MoveDown";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
        else if (deltaX > 0 && _currentAnimation != "MoveRight")
        {
            _currentAnimation = "MoveRight";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
        else if (deltaX < 0 && _currentAnimation != "MoveLeft")
        {
            _currentAnimation = "MoveLeft";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
        else if (deltaX == 0 && deltaY == 0 && _currentAnimation != "IdleDown")
        {
            _currentAnimation = "IdleDown";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
    }

}