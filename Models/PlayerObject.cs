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
        private SoundManager swordSoundManager;
        private static SoundManager backgroundMusicManager; // Static to ensure one instance

        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            if (backgroundMusicManager == null)
            {
                backgroundMusicManager = new SoundManager("C:\\Users\\A1\\Desktop\\PULL2\\TheAdventure\\Assets\\muzica.mp3");
                backgroundMusicManager.Play(true); // Start playing the background sound in a loop
            }

            swordSoundManager = new SoundManager("C:\\Users\\A1\\Desktop\\PULL2\\TheAdventure\\Assets\\Sword.mp3");
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
            swordSoundManager.Play();
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

            var x = Position.X + (int)(right * pixelsToMove) - (int)(left * pixelsToMove);
            var y = Position.Y - (int)(up * pixelsToMove) + (int)(down * pixelsToMove);

            x = Math.Clamp(x, 10, width - 10);
            y = Math.Clamp(y, 24, height - 6);

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

            Position = (x, y);
        }
    }
}
