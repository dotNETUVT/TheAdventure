using System.Formats.Asn1;
using Silk.NET.Maths;
using Silk.NET.SDL;
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



    public int health { get; private set; } = 100;
    public int maxHealth = 100;
    public double stamina { get; private set; } = 100;
    public int maxStamina = 100;
    public int mana { get; private set; } = 100;
    public int maxMana = 100;

    private const double StaminaConsumptionRate = 0.5;
    private const double StaminaRegenerationRate = 1;

    private bool _isRechargingStamina = false;



    public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(PlayerState.Idle, PlayerStateDirection.Down);

    }

    public void ApplyDamage(int damage)
    {

        health -= damage;


        if (health <= 0)
        {
            GameOver();
        }
    }

    private void ConsumeStamina()
    {
        stamina -= StaminaConsumptionRate;
        if (stamina < 0) stamina = 0;
    }
    public void Regenerate()
    {
        int manaConsumed = 20;
        mana -= manaConsumed;
        if (mana <= 0)
        { mana = 0; }
        else

        {
            health += 20;
        }
        if (health >= maxHealth) { health = maxHealth; }

    }

    private void RegenerateStamina()
    {
        stamina += StaminaRegenerationRate;
        if (stamina > maxStamina) stamina = maxStamina;
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

        if (_isRechargingStamina)
        {
            RegenerateStamina();
            if (stamina >= maxStamina)
            {
                _isRechargingStamina = false;
            }
            SetState(PlayerState.Idle, State.Direction);
            return;
        }

        if (stamina <= 0)
        {
            _isRechargingStamina = true;
            SetState(PlayerState.Idle, PlayerStateDirection.None);
            RegenerateStamina();
            return;
        }
        if (State.State == PlayerState.GameOver) return;
        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            State.State == PlayerState.Idle)
        {

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
            RegenerateStamina();
            return;
        }
        ConsumeStamina();
        Position = (x, y);
    }

}