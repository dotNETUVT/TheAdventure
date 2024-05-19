using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class EnemyObject : PlayerObject
{
    private const int AttackRange = 5;
    private const int BasePixelsPerSecond = 80;

    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {
    }

    public void UpdateEnemyPosition(int playerX, int playerY, double deltaTime)
    {
        var randomFactor = (float)(1 + (new Random().NextDouble() * 0.2 - 0.1));
        var pixelsPerSecond = (int)(BasePixelsPerSecond * randomFactor);

        var directionX = playerX - Position.X;
        var directionY = playerY - Position.Y;

        var distanceToPlayer = Math.Sqrt(directionX * directionX + directionY * directionY);
        if (distanceToPlayer < AttackRange)
        {
            SetState(PlayerState.Attack, PlayerStateDirection.Down);
            return;
        }

        // Calculate normalized direction vector
        var normDirectionX = directionX / distanceToPlayer;
        var normDirectionY = directionY / distanceToPlayer;

        // Calculate movement components
        var moveX = (int)(pixelsPerSecond * deltaTime * normDirectionX);
        var moveY = (int)(pixelsPerSecond * deltaTime * normDirectionY);

        if (Math.Abs(moveX) < 1 && Math.Abs(moveY) >= 1)
        {
            moveX = Math.Sign(normDirectionX);
        }
        else if (Math.Abs(moveY) < 1 && Math.Abs(moveX) >= 1)
        {
            moveY = Math.Sign(normDirectionY);
        }

        // Apply movement
        Position = (Position.X + moveX, Position.Y + moveY);
        if (Math.Abs(moveX) > Math.Abs(moveY))
        {
            if (moveX > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Right);
            }
            else if (moveX < 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Left);
            }
        }
        else if (Math.Abs(moveX) < Math.Abs(moveY))
        {
            if (moveY > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Down);
            }
            else if (moveY < 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Up);
            }
        }
        else
            SetState(PlayerState.Idle, State.Direction);
    }
}