using Silk.NET.SDL;
using System;

namespace TheAdventure.Models
{
    public class TemporaryGameObject : RenderableGameObject
    {
        public double Ttl { get; init; }
        public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

        private DateTimeOffset _spawnTime;
        public Action<TemporaryGameObject> OnExpire { get; set; }

        public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, Action<TemporaryGameObject> onExpire, double angle = 0.0, Point rotationCenter = new())
            : base(spriteSheet, position, angle, rotationCenter)
        {
            Ttl = ttl;
            _spawnTime = DateTimeOffset.Now;
            OnExpire = onExpire;
        }

    }
}
