using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject
    {
        private Input _input;
        private Engine _engine;
        private int _pixelsPerSecond = 192;
        private string _currentAnimation = "IdleDown";

        public PlayerObject(SpriteSheet spriteSheet, int x, int y, Input input, Engine engine) : base(spriteSheet, (x, y))
        {
            SpriteSheet.ActivateAnimation(_currentAnimation);
            _input = input;
            _engine = engine;
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time, bool shift, bool space)
        {
            if (shift)
                _pixelsPerSecond = 384;
            else
                _pixelsPerSecond = 192;

            if (up <= double.Epsilon && down <= double.Epsilon && left <= double.Epsilon && right <= double.Epsilon && _currentAnimation == "IdleDown")
            {
                return;
            }

            var pixelsToMove = time * _pixelsPerSecond;

            var x = Position.X + (int)(right * pixelsToMove) - (int)(left * pixelsToMove);
            var y = Position.Y - (int)(up * pixelsToMove) + (int)(down * pixelsToMove);

            x = Math.Clamp(x, 10, width - 10);
            y = Math.Clamp(y, 24, height - 6);

            if (y < Position.Y && _currentAnimation != "MoveUp")
            {
                _currentAnimation = "MoveUp";
            }
            else if (y > Position.Y && _currentAnimation != "MoveDown")
            {
                _currentAnimation = "MoveDown";
            }
            else if (x > Position.X && _currentAnimation != "MoveRight")
            {
                _currentAnimation = "MoveRight";
            }
            else if (x < Position.X && _currentAnimation != "MoveLeft")
            {
                _currentAnimation = "MoveLeft";
            }
            else if (x == Position.X && y == Position.Y && _currentAnimation != "IdleDown")
            {
                _currentAnimation = "IdleDown";
            }

            SpriteSheet.ActivateAnimation(_currentAnimation);
            Position = (x, y);

            if (space)
            {
                int xOffset = 0;
                int yOffset = 0;

                if (_currentAnimation == "MoveUp")
                {
                    yOffset = -15;
                }
                else if (_currentAnimation == "MoveDown")
                {
                    yOffset = 15;
                }
                else if (_currentAnimation == "MoveLeft")
                {
                    xOffset = -15;
                }
                else if (_currentAnimation == "MoveRight")
                {
                    xOffset = 15;
                }

                int newX = Position.X + xOffset;
                int newY = Position.Y + yOffset;

                newX = Math.Clamp(newX, 0, width);
                newY = Math.Clamp(newY, 0, height);

                Position = (newX, newY);
            }
        }
    }
}
