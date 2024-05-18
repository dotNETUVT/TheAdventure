using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class StarObject : RenderableGameObject
{
    public double Ttl { get; init; }
    private DateTimeOffset _starInterval;

    public bool IsUnavailable => (DateTimeOffset.Now - _starInterval).TotalSeconds >= Ttl;

    public StarObject(SpriteSheet spriteSheet, (int X, int Y) position, double ttl, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _starInterval = DateTimeOffset.Now;
    }

    public override void Render(GameRenderer renderer, float scale = 1.0f)
    {
        base.Render(renderer, scale);
    }
}