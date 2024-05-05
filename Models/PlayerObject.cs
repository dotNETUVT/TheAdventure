using Silk.NET.Input;
using Silk.NET.Maths;
using TheAdventure.Models;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject
    {
        private const int RegularSpeed = 192;
        private const int SprintSpeed = 256;
        private bool _isSprinting = false;
        private bool _isSpacebarPressed = false; // Add a field to track spacebar press
        public bool IsSprinting => _isSprinting;

        public void ToggleSprint()
        {
            _isSprinting = !_isSprinting;
        }

        private int _pixelsPerSecond = RegularSpeed;
        private string _currentAnimation = "IdleDown";

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SpriteSheet.ActivateAnimation(_currentAnimation);
        }

        // Update the method signature to accept the spacebar press state
        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time, bool isSprinting, bool isSpacebarPressed)
        {
            // Calculate total movement for this frame
            var deltaX = (right - left) * time * (_pixelsPerSecond + (isSprinting ? SprintSpeed : 0));
            var deltaY = (down - up) * time * (_pixelsPerSecond + (isSprinting ? SprintSpeed : 0));

            // Update player's position
            var x = Position.X + (int)deltaX;
            var y = Position.Y - (int)deltaY;

            // Clamp the position to the screen boundaries
            x = Math.Clamp(x, 10, width - 10);
            y = Math.Clamp(y, 24, height - 6);

            // Set the new position
            Position = (x, y);

            // Update animation based on movement direction and spacebar input
            if (isSpacebarPressed && _currentAnimation != "Attack")
            {
                // Activate the "Attack" animation
                _currentAnimation = "Attack";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
            else if (deltaY < 0 && _currentAnimation != "MoveUp")
            {
                _currentAnimation = "MoveUp";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
            else if (deltaY > 0 && _currentAnimation != "MoveDown")
            {
                _currentAnimation = "MoveDown";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
            else if (deltaX > 0 && _currentAnimation != "MoveRight")
            {
                _currentAnimation = "MoveRight";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
            else if (deltaX < 0 && _currentAnimation != "MoveLeft")
            {
                _currentAnimation = "MoveLeft";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
            else if (deltaX == 0 && deltaY == 0 && _currentAnimation != "IdleDown")
            {
                _currentAnimation = "IdleDown";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
        }


        public void SetSprint(bool isSprinting)
        {
            _pixelsPerSecond = isSprinting ? SprintSpeed : RegularSpeed;
        }

        public void TriggerAttackAnimation()
        {
            // Check if attack animation is not already active
            if (_currentAnimation != "Attack")
            {
                // Activate the attack animation
                _currentAnimation = "Attack";
                SpriteSheet.ActivateAnimation(_currentAnimation);
            }
        }
    }
}
