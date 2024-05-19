using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public enum ObstacleType
    {
        Red,
        Blue,
        Yellow
    }

    public class ObstacleObject : RenderableGameObject
    {
        public ObstacleType ObstacleType { get; }

        public ObstacleObject(SpriteSheet spriteSheet, (int X, int Y) position, ObstacleType obstacleType)
            : base(spriteSheet, position)
        {
            ObstacleType = obstacleType;
        }
    }
}
