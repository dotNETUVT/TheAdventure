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
    private int _hearth_texture_id;
    private string _current_animation = "IdleDown";
    public int _health = 3;

    public (PlayerState State, PlayerStateDirection Direction) State{ get; private set; }

    private GameRenderer _renderer;
    public PlayerObject(GameRenderer renderer, SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_current_animation);

        _renderer = renderer;
        TextureInfo textureInfo;

        _hearth_texture_id = _renderer.LoadTexture("Assets/life.png", out textureInfo);  



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



        if (y < Position.Y){
            SetState(PlayerState.Move, PlayerStateDirection.Up);
        }
        if (y > Position.Y ){
            SetState(PlayerState.Move, PlayerStateDirection.Down);
        }
        if (x > Position.X ){
            SetState(PlayerState.Move, PlayerStateDirection.Right);
        }
        if (x < Position.X){
            SetState(PlayerState.Move, PlayerStateDirection.Left);
        }
        if (x == Position.X &&
            y == Position.Y){
            SetState(PlayerState.Idle, State.Direction);
        }

        Position = (x, y);
    }
    public void RenderHearths(GameRenderer render)
    {
        int heartWidth = 32;
        int heartHeight = 32;
        int margin = 10;

        for (int i = 0; i < _health; i++)
        {   
            int screenX = margin + i * (heartWidth + 5);
            int screenY = margin;

            render.RenderTextureToScreen(_hearth_texture_id, new Rectangle<int>(0, 0, 32, 32), new Rectangle<int>(screenX, screenY, heartWidth, heartHeight));

        }
    }

    public int getHealth()
    {
        return _health;
    }

    public void setHealth(int health)
    {
        _health = health;
    }
}