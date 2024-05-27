using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;
using System;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject, IDisposable
    {
        public enum PlayerStateDirection
        {
            None = 0,
            Down,
            Up,
            Left,
            Right,
        }

        public enum PlayerState
        {
            None = 0,
            Idle,
            Move,
            Attack,
            GameOver
        }

        private int _pixelsPerSecond = 192;
        private int _lives;
        private AudioManager _audioManager;
        private (int X, int Y) _initialPosition;

        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public PlayerObject(SpriteSheet spriteSheet, int x, int y, string gameOverSoundPath, string lifeLostSoundPath, int initialLives = 3) : base(spriteSheet, (x, y))
        {
            _initialPosition = (x, y);
            _lives = initialLives;
            _audioManager = new AudioManager(gameOverSoundPath, lifeLostSoundPath);
            SetState(PlayerState.Idle, PlayerStateDirection.Down);
        }

        public void SetState(PlayerState state, PlayerStateDirection direction)
        {
            if (State.State == PlayerState.GameOver) return;
            if (State.State == state && State.Direction == direction)
            {
                return;
            }
            else if (state == PlayerState.None && direction == PlayerStateDirection.None)
            {
                SpriteSheet.ActivateAnimation(null);
            }
            else if (state == PlayerState.GameOver)
            {
                SpriteSheet.ActivateAnimation(Enum.GetName(state));
            }
            else
            {
                var animationName = Enum.GetName<PlayerState>(state) + Enum.GetName<PlayerStateDirection>(direction);
                SpriteSheet.ActivateAnimation(animationName);
            }
            State = (state, direction);
        }

        public void GameOver()
        {
            if (_lives > 0)
            {
                _lives--;
                if (_lives == 0)
                {
                    SetState(PlayerState.GameOver, PlayerStateDirection.None);
                    _audioManager.PlayGameOverSound();
                }
                else
                {
                    // Reset player position and play life lost sound
                    Position = _initialPosition;
                    _audioManager.PlayLifeLostSound();
                    // Optionally, reset the state to Idle or another appropriate state
                    SetState(PlayerState.Idle, PlayerStateDirection.Down);
                }
            }
        }

        public void LoseLife()
        {
            GameOver();
        }

        public void Attack(bool up, bool down, bool left, bool right)
        {
            if (State.State == PlayerState.GameOver) return;
            var direction = State.Direction;
            if (up)
            {
                direction = PlayerStateDirection.Up;
            }
            else if (down)
            {
                direction = PlayerStateDirection.Down;
            }
            else if (right)
            {
                direction = PlayerStateDirection.Right;
            }
            else if (left)
            {
                direction = PlayerStateDirection.Left;
            }
            SetState(PlayerState.Attack, direction);
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
        {
            if (State.State == PlayerState.GameOver) return;
            if (up <= double.Epsilon &&
                down <= double.Epsilon &&
                left <= double.Epsilon &&
                right <= double.Epsilon &&
                State.State == PlayerState.Idle)
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

            if (y < Position.Y)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Up);
            }
            if (y > Position.Y)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Down);
            }
            if (x > Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Right);
            }
            if (x < Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Left);
            }
            if (x == Position.X && y == Position.Y)
            {
                SetState(PlayerState.Idle, State.Direction);
            }
            Position = (x, y);
        }

        public void Dispose()
        {
            _audioManager.Dispose();
        }
    }
}
