using Silk.NET.Input;
using System;
// using System.Diagnostics;

namespace TheAdventure.Models;

internal class SecondPlayerObject: RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";


    public SecondPlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);

    }

    public void UpdateSecondPlayerPosition(double wKey, double aKey,double sKey, double dKey, int width, int height,
        double time)
    {
        // Debug.Print($"wKey={wKey}; aKey={aKey} ; sKey={sKey} ; dKey={dKey}");

        if (wKey <= double.Epsilon &&
            sKey <= double.Epsilon &&
            aKey <= double.Epsilon &&
            dKey <= double.Epsilon &&
            _currentAnimation == "IdleDown")
        {
            return;
        }
        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(dKey * pixelsToMove);
        x -= (int)(aKey * pixelsToMove);

        var y = Position.Y - (int)(wKey * pixelsToMove);
        y += (int)(sKey * pixelsToMove);

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

        if (y < Position.Y && _currentAnimation != "SwordUp")
        {
            // Console.WriteLine("SwordUp");
            _currentAnimation = "SwordUp";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (y > Position.Y && _currentAnimation != "SwordDown")
        {
            // Console.WriteLine("SwordDown");
            _currentAnimation = "SwordDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x > Position.X && _currentAnimation != "CrawlRight")
        {
            // Console.WriteLine("Crawl Right");
            _currentAnimation = "CrawlRight";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x < Position.X && _currentAnimation != "MoveLeft")
        {
            // Console.WriteLine("MoveLeft");
            _currentAnimation = "MoveLeft";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x == Position.X && _currentAnimation != "IdleDown" &&
            y == Position.Y && _currentAnimation != "IdleDown")
        {
            _currentAnimation = "IdleDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }

        //Console.WriteLine($"Will to switch to {_currentAnimation}");
        SpriteSheet.ActivateAnimation(_currentAnimation);
        Position = (x, y);
    }
}
