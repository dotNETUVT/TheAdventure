using Silk.NET.Maths;
using TheAdventure.Models;

namespace TheAdventure.Models
{
    public class PotionObject : RenderableGameObject
    {
        public PotionObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
        }
    }
}