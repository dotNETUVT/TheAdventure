using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

    public bool IsDestroying = false;
    
    private DateTimeOffset _spawnTime;
    
    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, bool isDestroying, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
        IsDestroying = isDestroying;
    }
}