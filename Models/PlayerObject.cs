using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    public int Health { get; private set; } = 50;
    public int heals = 30;
    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("IdleDown");
       
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time, bool fast)
    {
        var pixelsToMove = 0;
        if(fast)
            {pixelsToMove = (int)(time * _pixelsPerSecond * 2); }
        else 
            {pixelsToMove = (int)(time * _pixelsPerSecond); }

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


        //Console.WriteLine($"Will to switch to {_currentAnimation}");
        SpriteSheet.ActivateAnimation("IdleDown");
        Position = (x, y);
        
    }
    public void Heal(int healAmount)
    {
        Health = Math.Min(Health + healAmount, 100);
    }

    
}