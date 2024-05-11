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

    private int _pixelsPerSecond = 192;
    private bool _disabledUp;
    private bool _disabledDown;
    private bool _disabledLeft;
    private bool _disabledRight;

    public (PlayerState State, PlayerStateDirection Direction) State{ get; private set; }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(PlayerState.Idle, PlayerStateDirection.Down);
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

    public bool CheckAttackCollision(RenderableGameObject gameObject)
    {
        var target = gameObject.GetBoundingBox();

        var offsetX = 0;
        var offsetY = 0;
        if (State.Direction == PlayerStateDirection.Down)
        {
            offsetY = 15;
        }else if (State.Direction == PlayerStateDirection.Up)
        {
            offsetY = -15;
        }else if (State.Direction == PlayerStateDirection.Left)
        {
            offsetX = -15;
        }
        else
        {
            offsetX = 15;
        }
        
        var attackBox = new Rectangle<int>(
            Position.X - SpriteSheet.FrameCenter.OffsetX / 2 + offsetX, 
            Position.Y - SpriteSheet.FrameCenter.OffsetY / 2 + offsetY, 
            20, 20);
        
        if (attackBox.Origin.X < target.Origin.X + target.Size.X &&
            attackBox.Origin.X + attackBox.Size.X > target.Origin.X &&
            attackBox.Origin.Y + attackBox.Size.Y > target.Origin.Y &&
            attackBox.Origin.Y < target.Origin.Y + target.Size.Y)
        {
            return true;
        }

        return false;
    }
    
    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time)
    {
        if(State.State == PlayerState.GameOver) return;
        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            State.State == PlayerState.Idle){
            return;
        }

        var pixelsToMove = time * _pixelsPerSecond;

        int x = Position.X;
        if (!_disabledRight)
        {
            x += (int)(right * pixelsToMove);
        }
        if (!_disabledLeft)
        {
            x -= (int)(left * pixelsToMove);
        }

        int y = Position.Y;
        if (!_disabledDown)
        {
            y += (int)(down * pixelsToMove);
        }
        if (!_disabledUp)
        {
            y -= (int)(up * pixelsToMove);
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


        if (up > double.Epsilon){
            SetState(PlayerState.Move, PlayerStateDirection.Up);
        }
        if (down > double.Epsilon){
            SetState(PlayerState.Move, PlayerStateDirection.Down);
        }
        if (right > double.Epsilon){
            SetState(PlayerState.Move, PlayerStateDirection.Right);
        }
        if (left > double.Epsilon){
            SetState(PlayerState.Move, PlayerStateDirection.Left);
        }
        if (x == Position.X &&
            y == Position.Y){
            SetState(PlayerState.Idle, State.Direction);
        }

        Position = (x, y);
    }

    public void DisableMovementDirection(List<PlayerStateDirection> directions)
    {
        if (directions[0] == PlayerStateDirection.None)
        {
            _disabledUp = false;
            _disabledDown = false;
            _disabledLeft = false;
            _disabledRight = false;
            return;
        }

        foreach (var direction in directions)
        {
            switch (direction)
            {
                case PlayerStateDirection.Down: _disabledDown = true; break;
                case PlayerStateDirection.Up: _disabledUp = true; break;
                case PlayerStateDirection.Left: _disabledLeft = true; break;
                case PlayerStateDirection.Right: _disabledRight = true; break;
            }
        }
    }
    
    public override Rectangle<int> GetBoundingBox()
    {
        return new Rectangle<int>(
            Position.X - SpriteSheet.FrameCenter.OffsetX / 2, 
            Position.Y - SpriteSheet.FrameCenter.OffsetY / 2, 
            20, 20);
    }

    public Rectangle<int> GetAttackBoundingBox()
    {
        var offsetX = 0;
        var offsetY = 0;
        if (State.Direction == PlayerStateDirection.Down)
        {
            offsetY = 15;
        }else if (State.Direction == PlayerStateDirection.Up)
        {
            offsetY = -15;
        }else if (State.Direction == PlayerStateDirection.Left)
        {
            offsetX = -15;
        }
        else
        {
            offsetX = 15;
        }
        
        var attackBox = new Rectangle<int>(
            Position.X - SpriteSheet.FrameCenter.OffsetX / 2 + offsetX, 
            Position.Y - SpriteSheet.FrameCenter.OffsetY / 2 + offsetY, 
            20, 20);

        return attackBox;
    }
}