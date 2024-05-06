using Silk.NET.Maths;
using System;
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

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double speed)
    {
        bool isCurrentlyMoving = up > double.Epsilon || down > double.Epsilon || left > double.Epsilon || right > double.Epsilon;

        if (!isCurrentlyMoving && _currentAnimation == "IdleDown")
        {
            return;
        }

        var pixelsToMove = speed * _pixelsPerSecond;
        var x = Position.X + (int)(right * pixelsToMove) - (int)(left * pixelsToMove);
        var y = Position.Y + (int)(down * pixelsToMove) - (int)(up * pixelsToMove);

        // Updated bounds using Clamp
        x = Math.Clamp(x, 10, width - 10);
        y = Math.Clamp(y, 24, height - 6);

        UpdateAnimation(x, y);  // Modularity

        Position = (x, y);
    }

    private void UpdateAnimation(int x, int y)
    {
        if (y < Position.Y && _currentAnimation != "MoveUp")
            _currentAnimation = "MoveUp";
        else if (y > Position.Y && _currentAnimation != "MoveDown")
            _currentAnimation = "MoveDown";
        else if (x > Position.X && _currentAnimation != "MoveRight")
            _currentAnimation = "MoveRight";
        else if (x < Position.X && _currentAnimation != "MoveLeft")
            _currentAnimation = "MoveLeft";
        else if (x == Position.X && y == Position.Y && _currentAnimation != "IdleDown")
            _currentAnimation = "IdleDown";

        SpriteSheet.ActivateAnimation(_currentAnimation);
    }
}