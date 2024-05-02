using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private float _maxSpeed = 192; 
    private float _acceleration = 300f; 
    private float _friction = 0.9f; 
    private Vector2D<float> _velocity = new Vector2D<float>(0, 0); 

    private string _currentAnimation = "IdleDown";

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
    {
        Vector2D<float> input = new Vector2D<float>((float)(right - left), (float)(down - up));
        bool isInput = input.X != 0 || input.Y != 0;

        if (isInput)
        {
            _velocity += input * _acceleration * (float)time;
        }

        if (_velocity.Length > _maxSpeed)
        {
            _velocity = Vector2D.Normalize(_velocity) * _maxSpeed;
        }

        if (!isInput)
        {
            _velocity *= _friction;
        }

        if (_velocity.Length > 0.1)
        {
            Vector2D<int> newPosition = new Vector2D<int>(
                Position.X + (int)(_velocity.X * time),
                Position.Y + (int)(_velocity.Y * time));

            newPosition.X = Math.Clamp(newPosition.X, 10, width - 10);
            newPosition.Y = Math.Clamp(newPosition.Y, 24, height - 6);

            UpdateAnimation(newPosition.X, newPosition.Y);

            if (newPosition.X != Position.X || newPosition.Y != Position.Y)
            {
                Position = (newPosition.X, newPosition.Y);
            }
        }
        else
        {
            CheckIdleAnimation();
        }
    }

    private void UpdateAnimation(int newX, int newY)
    {
        if (newY < Position.Y && _currentAnimation != "MoveUp")
            _currentAnimation = "MoveUp";
        else if (newY > Position.Y && _currentAnimation != "MoveDown")
            _currentAnimation = "MoveDown";
        else if (newX > Position.X && _currentAnimation != "MoveRight")
            _currentAnimation = "MoveRight";
        else if (newX < Position.X && _currentAnimation != "MoveLeft")
            _currentAnimation = "MoveLeft";

        SpriteSheet.ActivateAnimation(_currentAnimation);
    }

    private void CheckIdleAnimation()
    {
        if (_currentAnimation != "IdleDown")
        {
            _currentAnimation = "IdleDown";
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }
    }
}
