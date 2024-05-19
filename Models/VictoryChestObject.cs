using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class VictoryChestObject : RenderableGameObject
    {
        public VictoryChestObject(SpriteSheet spriteSheet, (int X, int Y) position)
            : base(spriteSheet, position)
        {
        }
    }
}
