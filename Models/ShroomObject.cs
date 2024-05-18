using Silk.NET.Maths;
using TheAdventure.Models.Data;

namespace TheAdventure.Models
{
    public class ShroomObject : RenderableGameObject
    {
        private SpriteSheet _spriteSheet;
        private double _speed;
        private bool _right = true;

        public ShroomObject(SpriteSheet spriteSheet, int x, int y, double speed) : base(spriteSheet, (x, y))
        {
            _spriteSheet = spriteSheet;
            Position = (x, y);
            _speed = speed;
        }

        public void Update(double elapsedTime, int width)
        {
            if (Position.X >= 500)
            {
                _right = false;
            }

            if (Position.X <= 100)
            {
                _right = true;
            }


            // Check bounds and reverse direction if necessary
            if (_right)
            {
                _speed = Math.Abs(_speed);
            }
            else
            {
                _speed = -Math.Abs(_speed);
            }

            // Simple movement logic (e.g., moving horizontally)
            double pixelsToMove = _speed * elapsedTime;
            int x = Position.X + (int)(pixelsToMove * 1.0);

            if (x < 10)
            {
                x = 10;
            }

            if (x > width - 10)
            {
                x = width - 10;
            }

            Position = (x, Position.Y);
        }

        public override void Render(GameRenderer renderer)
        {
            _spriteSheet.Render(renderer, (Position.X, Position.Y));
        }

        public bool CheckCollision(PlayerObject player)
        {

            var deltaX = Math.Abs(player.Position.X - Position.X);
            var deltaY = Math.Abs(player.Position.Y - Position.Y);

            return deltaX < 32 && deltaY < 32;
        }
    }
}
