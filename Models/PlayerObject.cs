using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure.Models.Data;

namespace TheAdventure.Models
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

    public class PlayerObject : RenderableGameObject
    {
        private int _pixelsPerSecond = 192;
        public bool IsAlive { get; private set; } = true;
        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            State = (PlayerState.Idle, PlayerStateDirection.Down);
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (State.State == PlayerState.GameOver)
            {
                SpriteSheet.ActivateAnimation(Enum.GetName(typeof(PlayerState), PlayerState.GameOver));
            }
            else if (State.State != PlayerState.None)
            {
                var animationName = Enum.GetName(typeof(PlayerState), State.State) + Enum.GetName(typeof(PlayerStateDirection), State.Direction);
                SpriteSheet.ActivateAnimation(animationName);
            }
            else
            {
                SpriteSheet.ActivateAnimation(null);
            }
        }

        public void SetState(PlayerState state, PlayerStateDirection direction)
        {
            if (State.State == PlayerState.GameOver) return;
            if (State.State == state && State.Direction == direction) return;

            State = (state, direction);
            UpdateAnimation();
        }

        public void Attack(bool up, bool down, bool left, bool right)
        {
            if (State.State == PlayerState.GameOver) return;

            var direction = State.Direction;
            if (up)
                direction = PlayerStateDirection.Up;
            else if (down)
                direction = PlayerStateDirection.Down;
            else if (left)
                direction = PlayerStateDirection.Left;
            else if (right)
                direction = PlayerStateDirection.Right;

            SetState(PlayerState.Attack, direction);
        }

        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
        {
            if (State.State == PlayerState.GameOver) return;

            var pixelsToMove = time * _pixelsPerSecond;

            var newX = Position.X + (int)(right * pixelsToMove) - (int)(left * pixelsToMove);
            var newY = Position.Y - (int)(up * pixelsToMove) + (int)(down * pixelsToMove);

            // Apply boundaries to prevent the player from moving out of the visible area
            newX = Math.Clamp(newX, 10, width - 10);
            newY = Math.Clamp(newY, 24, height - 6);

            // Determine new state based on movement
            PlayerStateDirection direction = State.Direction;
            if (newY < Position.Y) direction = PlayerStateDirection.Up;
            if (newY > Position.Y) direction = PlayerStateDirection.Down;
            if (newX > Position.X) direction = PlayerStateDirection.Right;
            if (newX < Position.X) direction = PlayerStateDirection.Left;

            PlayerState newState = (newX == Position.X && newY == Position.Y) ? PlayerState.Idle : PlayerState.Move;

            SetState(newState, direction);
            Position = (newX, newY);
        }

        public void HitByExplosion()
        {
            IsAlive = false;
            SetState(PlayerState.GameOver, PlayerStateDirection.None);
        }
    }
}
