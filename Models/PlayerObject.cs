using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    public enum PlayerStateDirection{
        None = 0,
        Down,
        Up,
        Left,
        Right,
    }
    public enum PlayerState{
        None = 0,
        Idle,
        Move,
        Attack,
        GameOver
    }

    private float _pixelsPerSecond = 192f;
    private float _currentSpeed = 0f;
    private float _acceleration = 50f; 
    private float _deceleration = 70f; 

    public (PlayerState State, PlayerStateDirection Direction) State{ get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(PlayerState.Idle, PlayerStateDirection.Down);
    }

    public void SetState(PlayerState state, PlayerStateDirection direction)
    {
        if(State.State == PlayerState.GameOver) return;
        if(State.State == state && State.Direction == direction) return;

        var animationName = state == PlayerState.None ? null : Enum.GetName(typeof(PlayerState), state) + Enum.GetName(typeof(PlayerStateDirection), direction);
        SpriteSheet.ActivateAnimation(animationName);
        State = (state, direction);
    }

    public void GameOver(){
        SetState(PlayerState.GameOver, PlayerStateDirection.None);
    }

    public void Attack(bool up, bool down, bool left, bool right)
    {
        if(State.State == PlayerState.GameOver) return;
        var direction = DetermineDirection(up, down, left, right);
        SetState(PlayerState.Attack, direction);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
    {
        if(State.State == PlayerState.GameOver) return;

        var direction = DetermineDirection(up > double.Epsilon, down > double.Epsilon, left > double.Epsilon, right > double.Epsilon);
        if (direction != PlayerStateDirection.None && State.State != PlayerState.Attack)
        {
            _currentSpeed += _acceleration * (float)time;
            if (_currentSpeed > _pixelsPerSecond)
                _currentSpeed = _pixelsPerSecond;
        }
        else
        {
            _currentSpeed -= _deceleration * (float)time;
            if (_currentSpeed < 0)
                _currentSpeed = 0;
        }

        var movement = _currentSpeed * (float)time;
        var x = Position.X + (int)((right - left) * movement);
        var y = Position.Y - (int)((up - down) * movement);

        x = Math.Clamp(x, 10, width - 10);
        y = Math.Clamp(y, 24, height - 6);

        if (State.Direction != direction)
            SetState(State.State, direction);

        if (_currentSpeed == 0)
            SetState(PlayerState.Idle, direction);

        Position = (x, y);
    }

    private PlayerStateDirection DetermineDirection(bool up, bool down, bool left, bool right)
    {
        if (up && right) return PlayerStateDirection.Up;    
        if (up && left) return PlayerStateDirection.Up;
        if (down && right) return PlayerStateDirection.Down;
        if (down && left) return PlayerStateDirection.Down;
        if (up) return PlayerStateDirection.Up;
        if (down) return PlayerStateDirection.Down;
        if (left) return PlayerStateDirection.Left;
        if (right) return PlayerStateDirection.Right;
        return PlayerStateDirection.None;
    }
}




