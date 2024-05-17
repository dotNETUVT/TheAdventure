using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models
{
    public class TorchObject : RenderableGameObject
    {
        public TorchObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SpriteSheet.ActivateAnimation("NoBurn");
        }

        public void UpdateTorchPosition(int x, int y)
        {
            Position = (x, y);
        }
    }
}