using System.Drawing;
using TheAdventure;
using TheAdventure.Models;

public class Star : RenderableGameObject
{
    public Star(SpriteSheet spriteSheet, (int X, int Y) position)
        : base(spriteSheet, position)
    {
    }

    public override void Render(GameRenderer renderer)
    {
        SpriteSheet.Render(renderer, Position, Angle, RotationCenter);
    }
    
}