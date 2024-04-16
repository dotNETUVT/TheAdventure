using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

// activeaza animatia si da update la pozitie
public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    public bool canMoveUp = true;
    public bool canMoveDown = true;
    public bool canMoveLeft = true;
    public bool canMoveRight = true;
    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("IdleDown");
    }

    //public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
    //    double time)
    //{
    //    var pixelsToMove = time * _pixelsPerSecond;

    //    var x = Position.X + (int)(right * pixelsToMove);
    //    x -= (int)(left * pixelsToMove);

    //    var y = Position.Y - (int)(up * pixelsToMove);
    //    y += (int)(down * pixelsToMove);

    //    if (x < 10)
    //    {
    //        x = 10;
    //    }

    //    if (y < 24)
    //    {
    //        y = 24;
    //    }

    //    if (x > width - 10)
    //    {
    //        x = width - 10;
    //    }

    //    if (y > height - 6)
    //    {
    //        y = height - 6;
    //    }            
    //    Position = (x, y);
    //}

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
    {
        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        if (!canMoveUp && up > 0)
        {
            y = Math.Max(Position.Y, 24);
        }
        if (!canMoveDown && down > 0)
        {
            y = Math.Max(Position.Y, 24);
        }
        if (!canMoveLeft && left > 0)
        {
            x = Math.Max(Position.X, 10);
        }
        if (!canMoveRight && right > 0)
        {
            x = Math.Max(Position.X, 10);
        }

        x = Math.Max(Math.Min(x, width - 10), 10);
        y = Math.Max(Math.Min(y, height - 6), 24);

        Position = (x, y);
    }

}