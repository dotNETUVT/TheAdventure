using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

    private DateTimeOffset _spawnTime;
    private bool _isExploded = false; // Flag to check if the bomb has exploded
    public bool IsExploded => _isExploded; // Public property to access _isExploded

    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
    }

    public void Explode()
    {
        if (!_isExploded)
        {
            SpriteSheet.ActivateAnimation("Explode");
            _isExploded = true;
        }
    }
}

