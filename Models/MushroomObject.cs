using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class MushroomObject : RenderableGameObject
{
    public double Ttl { get; init; }
    private DateTimeOffset _spawnTime;

    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

    public MushroomObject(SpriteSheet spriteSheet, (int X, int Y) position, double ttl, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
    }

    public override void Render(GameRenderer renderer, float scale = 0.2f)
    {
        base.Render(renderer, scale);
    }
}
