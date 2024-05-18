using Silk.NET.Maths;
using TheAdventure.Models.Data;
using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class Treasure : RenderableGameObject
    {
        public bool Open { get; set; }

        public Treasure(SpriteSheet spriteSheet, (int X, int Y) position, double angle = 0.0, Point rotationCenter = new())
            : base(spriteSheet, position, angle, rotationCenter)
        {
            Open = false;
        }
    }
}