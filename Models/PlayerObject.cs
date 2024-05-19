using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";

    // Speed variables for manipulating the speed variation
    private double _baseSpeed = 150; //default speed
    private double _currentSpeed;

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
        _currentSpeed = _baseSpeed;
       
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int maxX, int maxY, double deltaTime)
    {
        var newPosition = Position;

        if (up > 0) newPosition.Y -= (int)(_currentSpeed * deltaTime);
        if (down > 0) newPosition.Y += (int)(_currentSpeed * deltaTime);
        if (left > 0) newPosition.X -= (int)(_currentSpeed * deltaTime);
        if (right > 0) newPosition.X += (int)(_currentSpeed * deltaTime);

        newPosition.X = Math.Clamp(newPosition.X, 0, maxX);
        newPosition.Y = Math.Clamp(newPosition.Y, 0, maxY);

        Position = newPosition;
    }

    public void IncreaseSpeed(double factor)
    {
        // Increase speed based on the factor that is provided in the code, my case = 3.0 (tripled it, for the player to run)
        _currentSpeed = _baseSpeed * factor;
    }

    public void ResetSpeed()
    {
        // Reset the speed for when Tab is released
        _currentSpeed = _baseSpeed;
    }

}