using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;
using System.Media;

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

    private bool _isSprinting = false;
    public bool IsSprinting => _isSprinting;
    private const int SprintSpeed = 300;
    private int _pixelsPerSecond = 192;
    private Input _input;
    private SoundPlayer soundPlayer;



    public (PlayerState State, PlayerStateDirection Direction) State{ get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y, Input input) : base(spriteSheet, (x, y))
    {
        string filePath = @"Assets\sword_slash.wav";
        soundPlayer = new SoundPlayer(filePath);
        soundPlayer.LoadAsync();
        _input = input; 
        SetState(PlayerState.Idle, PlayerStateDirection.Down);

    }

    public void ToggleSprint()
    {
        _isSprinting = !_isSprinting;
    }

    public void SetState(PlayerState state, PlayerStateDirection direction)
    {
        if(State.State == PlayerState.GameOver) return;
        if(State.State == state && State.Direction == direction){
            return;
        }
        else if(state == PlayerState.None && direction == PlayerStateDirection.None){
            SpriteSheet.ActivateAnimation(null);
        }
        else if(state == PlayerState.GameOver){
            SpriteSheet.ActivateAnimation(Enum.GetName(state));
        }
        else{
            var animationName = Enum.GetName<PlayerState>(state) + Enum.GetName<PlayerStateDirection>(direction);
            SpriteSheet.ActivateAnimation(animationName);
        }
        State = (state, direction);
    }

    public void GameOver(){
        SetState(PlayerState.GameOver, PlayerStateDirection.None);
    }

    public void Attack(bool up, bool down, bool left, bool right)
    {
        if(State.State == PlayerState.GameOver) return;

        soundPlayer.Play();

        var direction = State.Direction;
        if(up){
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
        else if (left){
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

        bool isWPressed = _input.IsWPressed();
        _pixelsPerSecond = isWPressed ? SprintSpeed : _pixelsPerSecond; 

        var deltaX = (right - left) * time * _pixelsPerSecond;
        var deltaY = (down - up) * time * _pixelsPerSecond;

        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        x = Math.Clamp(x, 10, width - 10);
        y = Math.Clamp(y, 24, height - 6);

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


    public void SetSprint(bool isSprinting)
    {
        _pixelsPerSecond = isSprinting ? SprintSpeed : _pixelsPerSecond;
    }
}