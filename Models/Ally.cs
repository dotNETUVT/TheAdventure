using Silk.NET.Maths;
using TheAdventure.Models;

namespace TheAdventure
{
    public class Ally : RenderableGameObject
    {
        private PlayerObject _player;
        private int _speed;

        public Ally(SpriteSheet spriteSheet, PlayerObject player, int x, int y, int speed = 2)
            : base(spriteSheet, (x, y))
        {
            _player = player;
            _speed = speed;
            SpriteSheet.ActivateAnimation("Idle");
        }

        public void Update()
        {
            FollowPlayer();
        }

        private void FollowPlayer()
        {
            var direction = (X: _player.Position.X - Position.X, Y: _player.Position.Y - Position.Y);

            // Normalize the direction vector
            var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (length == 0)
            {
                SpriteSheet.ActivateAnimation("Idle");
                return; // Already at the player's position
            }

            var normalizedDirection = (X: direction.X / length, Y: direction.Y / length);

            // Determine the direction to set the correct animation
            if (Math.Abs(normalizedDirection.X) > Math.Abs(normalizedDirection.Y))
            {
                if (normalizedDirection.X > 0)
                {
                    SpriteSheet.ActivateAnimation("MoveRight");
                }
                else
                {
                    SpriteSheet.ActivateAnimation("MoveLeft");
                }
            }
            else
            {
                if (normalizedDirection.Y > 0)
                {
                    SpriteSheet.ActivateAnimation("MoveDown");
                }
                else
                {
                    SpriteSheet.ActivateAnimation("MoveUp");
                }
            }

            // Move the ally towards the player
            var newX = Position.X + (int)(normalizedDirection.X * _speed);
            var newY = Position.Y + (int)(normalizedDirection.Y * _speed);
            Position = (newX, newY);
        }
    }
}
