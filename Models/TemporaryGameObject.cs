using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;
    
    private DateTimeOffset _spawnTime;
    private double _speed;

    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, double speed, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
        _speed = speed;
    }

}
