using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    private bool hasCompanion = false;
    private CompanionObject companion;

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
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

        Position = (x, y);
    }

    public bool nearACompanion(CompanionObject companion)
    {   

        if(hasCompanion)
            return false;

        if(!companion.IsWildAnimal())
            return false;

        int distanceX = Position.X - companion.Position.X;
        int distanceY = Position.Y - companion.Position.Y;


        if ( (distanceX < 30 && distanceX > -30) && (distanceY < 30 && distanceY > -30) ){
            Console.WriteLine("Player is near a companion: " + companion.Position.X + " " + companion.Position.Y);
            return true;
        }
        
        return false;
    }


    public void SetHasCompanion(bool hasCompanion, CompanionObject? companion){
        this.hasCompanion = hasCompanion;
        this.companion = companion;
    }

    public bool GetHasCompanion(){
        return hasCompanion;
    }

    public CompanionObject GetCompanion(){
        return companion;
    }
}