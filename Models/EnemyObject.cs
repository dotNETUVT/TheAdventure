using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class EnemyObject : RenderableGameObject
{
    protected int _pixelsPerSecond = 192; 
    protected double _movementTimer = 0.0; 
    protected double _movementCooldown = 0.06;

    protected string _currentAnimation = "MoveDown";

    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
    }

    public virtual void UpdateAnimation(int xDiff, int yDiff)
    {
        // update enemy animation

        if (Math.Abs(xDiff) > Math.Abs(yDiff))
        {
            if (xDiff > 0)
            {
                _currentAnimation = "MoveRight";
            }
            else
            {
                _currentAnimation = "MoveLeft";
            }
        }
        else
        {
            if (yDiff > 0)
            {
                _currentAnimation = "MoveDown";
            }
            else
            {
                _currentAnimation = "MoveUp";
            }
        }

        SpriteSheet.ActivateAnimation(_currentAnimation);

    }

    public virtual void UpdateEnemyPosition((int x, int y) playerPosition, double time)
    {
        // zombie moves way slower than the player, so we need to update its position less frequently
        // in this case we update the zombie position every 2 seconds

        _movementTimer += time;
        if (_movementTimer >= _movementCooldown)
        {
            Console.WriteLine($"Zombie {Id} is moving");
            _movementTimer = 0.0;

            // update zombie position in the direction of the player
            var (x, y) = Position;

            var xDiff = playerPosition.x - x;
            var yDiff = playerPosition.y - y;

            var angle = Math.Atan2(yDiff, xDiff);

            var xMove = Math.Cos(angle) * _pixelsPerSecond;
            var yMove = Math.Sin(angle) * _pixelsPerSecond;

            x += (int)(xMove * time);
            y += (int)(yMove * time);

            Position = (x, y);

            UpdateAnimation(xDiff, yDiff);

        }

    }
    
}