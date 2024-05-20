using Silk.NET.SDL;
using System;

namespace TheAdventure.Models
{
    public enum GameObjectType
    {
        Bomb,
        // Add other types here if needed
    }

    public class TemporaryGameObject : RenderableGameObject
    {
        public GameObjectType Type { get; } // Add Type property to differentiate object types

        public double Ttl { get; init; }
        public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

        public bool IsExploding { get; set; } // Added property for explosion state

        private DateTimeOffset _spawnTime;

        // Constructor with all parameters
        public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, GameObjectType type, double angle, Point rotationCenter)
            : base(spriteSheet, position, angle, rotationCenter)
        {
            Ttl = ttl;
            _spawnTime = DateTimeOffset.Now;
            Type = type;
        }

        // Constructor with default rotationCenter
        public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, GameObjectType type, double angle = 0.0)
            : this(spriteSheet, ttl, position, type, angle, new Point())
        {
        }
    }
}
