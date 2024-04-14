using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("IdleDown");
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time, Dictionary<int, GameObject> gameObjects)
    {
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

        foreach (int i in gameObjects.Keys)
        {
            if (gameObjects[i] is RenderableGameObject)
            {
                string collision = CollidesWith((RenderableGameObject)gameObjects[i]);
                if (collision != "")
                {
                    x = Position.X;
                    y = Position.Y;

                    if (collision == "left")
                    {
                        x -= 1;
                    }
                    else if (collision == "right")
                    {
                        x += 1;
                    }
                    else if (collision == "up")
                    {
                        y -= 1;
                    }
                    else if (collision == "down")
                    {
                        y += 1;
                    }
                }
            }
        }
        
        Position = (x, y);
    }
}