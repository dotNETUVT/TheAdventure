using Silk.NET.Maths;
using TheAdventure;


namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    private SoundPlayer _moveSoundPlayer;
   

    public PlayerObject(SpriteSheet spriteSheet, int x, int y,string soundFilePath) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("IdleDown");
        _moveSoundPlayer = new SoundPlayer(soundFilePath);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
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
        if (up > 0)
        {
            SpriteSheet.ActivateAnimation("WalkingUp");
        }
        else if (down > 0)
        {
            SpriteSheet.ActivateAnimation("WalkingDown");
        }
        else if (left > 0)
        {
            SpriteSheet.ActivateAnimation("WalkingLeft");
        }
        else if (right > 0)
        {
            SpriteSheet.ActivateAnimation("WalkingRight");
        }
        else
        {
            SpriteSheet.ActivateAnimation("IdleDown"); 
        }
        if (up > 0 || down > 0 || left > 0 || right > 0)
        {
            _moveSoundPlayer.Play();
        }   
        else
        {
            _moveSoundPlayer.Stop();
        }

        Position = (x, y);
    }
}