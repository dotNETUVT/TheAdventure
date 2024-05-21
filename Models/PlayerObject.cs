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

    public int HP { get; private set; }
    public int Energy { get; private set; } = 4;
    private const int MaxEnergy = 4;
    private const double EnergyDepletionInterval = 1.0; // Deplete energy every 1 second
    private const double EnergyRegenerationInterval = 0.25; // Regenerate energy every 2 seconds
    private double _timeSinceLastEnergyDepletion = 0;
    private double _timeSinceLastEnergyRegeneration = 0;

    public PlayerObject(SpriteSheet spriteSheet, int x, int y, int initialHP) : base(spriteSheet, (x, y))
    {
        HP = initialHP;
        SetState(PlayerState.Idle, PlayerStateDirection.Down);
    }

    public void TakeDamage()
    {
        HP = HP - 50;
        if (HP <= 0)
        {
            GameOver();
        }
    }

    public void ResetHP()
    {
        HP = 3;
    }

    public void ResetEnergy()
    {
        Energy = MaxEnergy;
    }

    public void DepleteEnergy(double deltaTime)
    {
        _timeSinceLastEnergyDepletion += deltaTime;
        if (_timeSinceLastEnergyDepletion >= EnergyDepletionInterval)
        {
            if (Energy > 0)
            {
                Energy--;
            }
            _timeSinceLastEnergyDepletion = 0;
        }
    }

    public void RechargeEnergy(double deltaTime)
    {
        _timeSinceLastEnergyRegeneration += deltaTime;
        if (_timeSinceLastEnergyRegeneration >= EnergyRegenerationInterval)
        {
            if (Energy < MaxEnergy)
            {
                Energy++;
            }
            _timeSinceLastEnergyRegeneration = 0;
        }
    }

    public bool HasEnergy => Energy > 0;

    public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

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
}
