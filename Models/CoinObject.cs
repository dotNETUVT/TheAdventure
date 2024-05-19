using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class CoinObject : RenderableGameObject
    {
        public CoinObject(SpriteSheet spriteSheet, (int X, int Y) position)
            : base(spriteSheet, position)
        {
        }
    }
}
