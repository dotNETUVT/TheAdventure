using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;
    
    private DateTimeOffset _spawnTime;

    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
    }

    // Method to get the bounding box of the temporary game object
    public System.Drawing.Rectangle GetBoundingBox()
    {
        // Assuming the bounding box is a rectangle centered at its position with width and height of sprite
        return new System.Drawing.Rectangle((int)(Position.X - SpriteSheet.FrameWidth / 2), (int)(Position.Y - SpriteSheet.FrameHeight / 2), SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
    }
}