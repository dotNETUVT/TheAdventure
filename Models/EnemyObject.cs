using Silk.NET.Maths;
using TheAdventure.Models;

namespace TheAdventure
{
    public class EnemyObject : RenderableGameObject
    {
        public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SetIdleState();
        }

        private void SetIdleState()
        {
            SpriteSheet.ActivateAnimation("Idle");
        }
    }
}