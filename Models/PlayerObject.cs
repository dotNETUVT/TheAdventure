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
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    public enum PlayerState
    {
        None = 0,
        Idle,
        Move,
        Attack,
        Charge,
        GameOver
    }

    private int _pixelsPerSecond = 192;
    private bool _isCharging;
    private double _chargeDuration;
    private DateTimeOffset _chargeStartTime;
    private Vector2D<double> _chargeDirection;
    private (int X, int Y) _chargeTargetPosition;
    private DateTimeOffset _lastChargeTime = DateTimeOffset.MinValue;
    private readonly double _chargeCooldownDuration = 1.0; 
    private readonly int _dashDistanceMultiplier = 12; 

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

public void StartChargeAttack(double duration, bool up, bool down, bool left, bool right, int tileSize)
{
    if (State.State == PlayerState.GameOver) return;

    var currentTime = DateTimeOffset.Now;
    if ((currentTime - _lastChargeTime).TotalSeconds < _chargeCooldownDuration)
    {
        return; 
    }

    _isCharging = true;
    _chargeDuration = duration;
    _chargeStartTime = currentTime;
    _lastChargeTime = currentTime;

    _chargeDirection = new Vector2D<double>(
        right ? 1 : left ? -1 : 0,
        down ? 1 : up ? -1 : 0
    );

    var length = Math.Sqrt(_chargeDirection.X * _chargeDirection.X + _chargeDirection.Y * _chargeDirection.Y);

    if (length > 0)
    {
        _chargeDirection = new Vector2D<double>(_chargeDirection.X / length, _chargeDirection.Y / length);
    }

    _chargeTargetPosition = (
        Position.X + (int)(_chargeDirection.X * _dashDistanceMultiplier * tileSize),
        Position.Y + (int)(_chargeDirection.Y * _dashDistanceMultiplier * tileSize)
    );

    SetState(PlayerState.Charge, GetDirection(up, down, left, right));
    SetState(PlayerState.Attack, State.Direction); 
}

public void UpdateChargeAttack(double elapsedTime, int worldWidth, int worldHeight)
{
    if (_isCharging)
    {
        var timeElapsed = (DateTimeOffset.Now - _chargeStartTime).TotalSeconds;
        if (timeElapsed > _chargeDuration)
        {
            _isCharging = false;
            SetState(PlayerState.Idle, State.Direction);
        }
        else
        {
            var chargeSpeed = _dashDistanceMultiplier * 32 / _chargeDuration; 
            var newX = Position.X + (int)(_chargeDirection.X * chargeSpeed * elapsedTime);
            var newY = Position.Y + (int)(_chargeDirection.Y * chargeSpeed * elapsedTime);

            newX = Math.Clamp(newX, 0, worldWidth);
            newY = Math.Clamp(newY, 0, worldHeight);

            if ((_chargeDirection.X > 0 && newX >= _chargeTargetPosition.X) ||
                (_chargeDirection.X < 0 && newX <= _chargeTargetPosition.X) ||
                (_chargeDirection.Y > 0 && newY >= _chargeTargetPosition.Y) ||
                (_chargeDirection.Y < 0 && newY <= _chargeTargetPosition.Y))
            {
                Position = _chargeTargetPosition;
                _isCharging = false;
                SetState(PlayerState.Idle, State.Direction);
            }
            else
            {
                Position = (newX, newY);
            }
        }
    }
}

public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
{
    if (State.State == PlayerState.GameOver) return;
    if (_isCharging) return; 
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
    if (x == Position.X &&
        y == Position.Y)
    {
        SetState(PlayerState.Idle, State.Direction);
    }

    Position = (x, y);
}

private PlayerStateDirection GetDirection(bool up, bool down, bool left, bool right)
{
    if (up && left)
        return PlayerStateDirection.UpLeft;
    if (up && right)
        return PlayerStateDirection.UpRight;
    if (down && left)
        return PlayerStateDirection.DownLeft;
    if (down && right)
        return PlayerStateDirection.DownRight;
    if (up)
        return PlayerStateDirection.Up;
    if (down)
        return PlayerStateDirection.Down;
    if (left)
        return PlayerStateDirection.Left;
    if (right)
        return PlayerStateDirection.Right;
    return PlayerStateDirection.None;
}

public bool IsCharging => _isCharging;

}