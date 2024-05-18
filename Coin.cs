namespace TheAdventure.Models
{
    public class Coin : GameObject
    {
        public Coin(SpriteSheet spriteSheet, float lifespan, (int X, int Y) position)
            : base(spriteSheet, lifespan, position)
        {
        }
    }
}
