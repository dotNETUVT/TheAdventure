using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public enum PowerUpType
    {
        Red,
        Yellow,
        Blue,
        Victory
    }

    public class PowerUpObject : RenderableGameObject
    {
        public PowerUpType PowerUpType { get; }
        public ObstacleObject CorrespondingObstacle { get; set; }

        public PowerUpObject(SpriteSheet spriteSheet, (int X, int Y) position, PowerUpType powerUpType)
            : base(spriteSheet, position)
        {
            PowerUpType = powerUpType;
        }
    }
}
