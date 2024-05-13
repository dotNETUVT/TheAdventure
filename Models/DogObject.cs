namespace TheAdventure.Models;

public class DogObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    private string _lastDirection = "Stay";

    public DogObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("Stay");
    }

    public void UpdateDogPosition(double up, double down, double left, double right, int width, int height,
        double time)
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

        string newAnimation = null;
        if (up > 0)
        {
            newAnimation = "WalkUp";
        }
        else if (down > 0)
        {
            newAnimation = "WalkDown";
        }
        else if (left > 0)
        {
            newAnimation = "WalkLeft";
        }
        else if (right > 0)
        {
            newAnimation = "WalkRight";
        }
        else
        {
            newAnimation = _lastDirection.Replace("Walk", "Stay");
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

        if (newAnimation != _lastDirection || SpriteSheet.ActiveAnimation == null)
        {
            SpriteSheet.ActivateAnimation(newAnimation);
            _lastDirection = newAnimation;
        }

        Position = (x, y);
    }
}