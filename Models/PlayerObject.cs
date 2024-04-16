namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    public int lastDirection = 0; // 0 - up, 1 - right, 2 - down, 3 - left


    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        lastDirection = 0;
        SpriteSheet.ActivateAnimation("IdleDown");
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time)
    {
        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        //Bounds check
        if (x < 10) x = 10;
        if (y < 24) y = 24;
        if (x > width - 10) x = width - 10;
        if (y > height - 6) y = height - 6;

        // For Animations

        // if (up == 0 && right == 0 && down == 0 && left == 0)
        {
            // For Idling
            lastDirection = 4;
        }


        if (up > 0) lastDirection = 0;
        if (down > 0) lastDirection = 2;
        if (right > 0) lastDirection = 1;
        if (left > 0) lastDirection = 3;

        Position = (x, y);
    }
}