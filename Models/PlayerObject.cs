using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    public enum PlayerStateDirection
    {
        None = 0,
        Down,
        Up,
        Left,
        Right,
        DownLeft,
        DownRight,
        UpLeft,
        UpRight
    }

    public enum PlayerState
    {
        None = 0,
        Idle,
        Move,
        Attack,
        Run,
        GameOver
    }

    private int _pixelsPerSecond = 192;
    private float _runMultiplier = 1.3f;

    public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
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
        Console.WriteLine($"Player state changed to: {state}, direction: {direction}");
    }

    public void GameOver(){
        SetState(PlayerState.GameOver, PlayerStateDirection.None);
    }
    
    public void ForceState(PlayerState state, PlayerStateDirection direction)
    {
        var animationName = Enum.GetName<PlayerState>(state) + Enum.GetName<PlayerStateDirection>(direction);
        SpriteSheet.ActivateAnimation(animationName);
        State = (state, direction);
        Console.WriteLine($"Player state forcibly changed to: {state}, direction: {direction}");
    }

    public void Respawn(int x, int y)
    {
        ForceState(PlayerState.Idle, PlayerStateDirection.Down);
        Position = (x, y);
        Console.WriteLine("Player respawned at position (" + x + ", " + y + ")");
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
        double time, bool isRunning)
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

        var speed = isRunning ? _pixelsPerSecond * _runMultiplier : _pixelsPerSecond;
        var pixelsToMove = time * speed;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        if (x < 10)
        {
            x = 10;
        }

        const int skyHeight = 300;
        if (y < skyHeight + 24)
        {
            y = skyHeight + 24;
        }

        if (x > width - 10)
        {
            x = width - 10;
        }

        if (y > height - 6)
        {
            y = height - 6;
        }

        if (isRunning)
        {
            if (up > 0 && left > 0)
            {
                SetState(PlayerState.Run, PlayerStateDirection.UpLeft);
            }
            else if (up > 0 && right > 0)
            {
                SetState(PlayerState.Run, PlayerStateDirection.UpRight);
            }
            else if (down > 0 && left > 0)
            {
                SetState(PlayerState.Run, PlayerStateDirection.DownLeft);
            }
            else if (down > 0 && right > 0)
            {
                SetState(PlayerState.Run, PlayerStateDirection.DownRight);
            }
            else if (y < Position.Y)
            {
                SetState(PlayerState.Run, PlayerStateDirection.Up);
            }
            else if (y > Position.Y)
            {
                SetState(PlayerState.Run, PlayerStateDirection.Down);
            }
            else if (x > Position.X)
            {
                SetState(PlayerState.Run, PlayerStateDirection.Right);
            }
            else if (x < Position.X)
            {
                SetState(PlayerState.Run, PlayerStateDirection.Left);
            }
        }
        else
        {
            if (up > 0 && left > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.UpLeft);
            }
            else if (up > 0 && right > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.UpRight);
            }
            else if (down > 0 && left > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.DownLeft);
            }
            else if (down > 0 && right > 0)
            {
                SetState(PlayerState.Move, PlayerStateDirection.DownRight);
            }
            else if (y < Position.Y)
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
            if (x == Position.X && y == Position.Y)
            {
                SetState(PlayerState.Idle, State.Direction);
            }
        }
        Position = (x, y);
    }
}