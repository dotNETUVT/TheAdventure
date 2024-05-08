using System.Formats.Asn1;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure;

namespace TheAdventure.Models;

public class MagicStoneObject : RenderableGameObject
{
    public int Health { get; set; } = 100;
    public MagicStoneObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y)) { }

    public void takeHit()
    {
        if (Health > 0)
        {
            Health -= 5;
        }
    }
    public override void Render(GameRenderer renderer)
    {
        if (Health > 0)
        {
            base.Render(renderer); 
        }
    }

}