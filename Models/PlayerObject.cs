using Silk.NET.Maths;
using TheAdventure.Models.Data;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject
    {
        private int _pixelsPerSecond = 192;
        private string _currentAnimation = "IdleDown";

        public bool IsAlive { get; private set; } = true;

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
            double time)
        {
            if (up <= double.Epsilon &&
                down <= double.Epsilon &&
                left <= double.Epsilon &&
                right <= double.Epsilon &&
                _currentAnimation == "IdleDown")
            {
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

            if (y < Position.Y && _currentAnimation != "MoveUp")
            {
                _currentAnimation = "MoveUp";
            }
            if (y > Position.Y && _currentAnimation != "MoveDown")
            {
                _currentAnimation = "MoveDown";
            }
            if (x > Position.X && _currentAnimation != "MoveRight")
            {
                _currentAnimation = "MoveRight";
            }
            if (x < Position.X && _currentAnimation != "MoveLeft")
            {
                _currentAnimation = "MoveLeft";
            }
            if (x == Position.X && y == Position.Y && _currentAnimation != "IdleDown")
            {
                _currentAnimation = "IdleDown";
            }

            SpriteSheet.ActivateAnimation(_currentAnimation);
            Position = (x, y);
        }

        public void HitByExplosion()
        {
            IsAlive = false;
        }
    }
}
