using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;
public class EnemyObject : PlayerObject
{
    private int _basePixelPerSecond = 80;
    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {
    }
    public void UpdateEnemyPosition(int playerX, int playerY, double deltaTime)
    {
        var randomFactor = (float)(1 + (new Random().NextDouble() * 0.2 - 0.1));
        var pixelsPerSecond = (int)(_basePixelPerSecond * randomFactor);
        // Calculate movement towards the player
        var directionX = playerX - Position.X;
        var directionY = playerY - Position.Y;

        var distanceToPlayer = Math.Sqrt(directionX * directionX + directionY * directionY);
        if (distanceToPlayer == 0)
            return;
        // Normalize the direction vector
        var normDirectionX = directionX / distanceToPlayer;
        var normDirectionY = directionY / distanceToPlayer;

        // Calculate movement based on normalized direction and speed
        var moveX = (int)(pixelsPerSecond * deltaTime * normDirectionX);
        var moveY = (int)(pixelsPerSecond * deltaTime * normDirectionY);

        // Update enemy position
        Position = (Position.X + moveX, Position.Y + moveY);

        // Determine animation based on movement direction
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
