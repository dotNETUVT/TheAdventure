using System.Reflection.Metadata.Ecma335;
using Silk.NET.Maths;
using TheAdventure;

public class PlayerObject : AnimatedGameObject
{
    /// <summary>
    /// Player X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Player Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    private string direction = "down";

    public PlayerObject(int id) : base("character_idle_down.png", id, 2, 400, 400)
    {
        X = 400;
        Y = 400;
        UpdateScreenTarget();
    }

    private void UpdateScreenTarget(){
        var targetX = X + 24;
        var targetY = Y - 42;

        _target = new Rectangle<int>(targetX, targetY, 48, 48);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int time, string direc)
    {
        direction = direc;
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        var aux_x = X + (int)(right * pixelsToMove);
        aux_x -= (int)(left * pixelsToMove);

        var aux_y = Y - (int)(up * pixelsToMove);
        aux_y += (int)(down * pixelsToMove);

        if (!(aux_x < -32 || aux_y < 32) && !(aux_x > 900 || aux_y > 640-16))
        {
            Y = aux_y;
            X = aux_x;
        }

        setAnimationSpeed(4000);
        if (direction == "right")
        {
            ChangeAnimation("character_movement_right.png", Id, 6, X, Y);
        }
        else if (direction == "left")
        {
            ChangeAnimation("character_movement_left.png", Id, 6, X, Y);
        }
        else if (direction == "up")
        {
            ChangeAnimation("character_movement_up.png", Id, 4, X, Y);
        }
        else if (direction == "down")
        {
            ChangeAnimation("character_movement_down.png", Id, 4, X, Y);
        }
        UpdateScreenTarget();
    }

    public void SetIdleState()
    {
        setAnimationSpeed(2000);

        if (direction == "right")
        {
            ChangeAnimation("character_idle_right.png", Id, 2, X, Y);
        }
        else if (direction == "left")
        {
            ChangeAnimation("character_idle_left.png", Id, 2, X, Y);
        }
        else if (direction == "up")
        {
            ChangeAnimation("character_idle_up.png", Id, 2, X, Y);
        }
        else if (direction == "down")
        {
            ChangeAnimation("character_idle_down.png", Id, 2, X, Y);
        }
        UpdateScreenTarget();
    }


 
}