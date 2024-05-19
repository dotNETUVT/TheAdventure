using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models
{
    public class PlayerObject : RenderableGameObject
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
        private (int X, int Y) _previousPosition;

        public bool CanMoveUp { get; set; } = true;
        public bool CanMoveDown { get; set; } = true;
        public bool CanMoveLeft { get; set; } = true;
        public bool CanMoveRight { get; set; } = true;

        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SetState(PlayerState.Idle, PlayerStateDirection.Down);
            _previousPosition = (x, y);
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
            SetState(PlayerState.GameOver, PlayerStateDirection.None);
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

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
            double time)
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

            var x = Position.X;
            var y = Position.Y;

            if (up > double.Epsilon && CanMoveUp)
            {
                y -= (int)(up * pixelsToMove);
            }

            if (down > double.Epsilon && CanMoveDown)
            {
                y += (int)(down * pixelsToMove);
            }

            if (left > double.Epsilon && CanMoveLeft)
            {
                x -= (int)(left * pixelsToMove);
            }

            if (right > double.Epsilon && CanMoveRight)
            {
                x += (int)(right * pixelsToMove);
            }

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
            else if (y > Position.Y)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Down);
            }
            else if (x > Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Right);
            }
            else if (x < Position.X)
            {
                SetState(PlayerState.Move, PlayerStateDirection.Left);
            }
            else
            {
                SetState(PlayerState.Idle, State.Direction);
            }

            _previousPosition = Position;
            Position = (x, y);
        }

        public void MoveBackToPreviousPosition()
        {
            Position = _previousPosition;
        }

        public void ResetMovementFlags()
        {
            CanMoveUp = true;
            CanMoveDown = true;
            CanMoveLeft = true;
            CanMoveRight = true;
        }
    }
}