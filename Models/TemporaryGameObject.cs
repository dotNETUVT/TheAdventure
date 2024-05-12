using System.Security.Cryptography.X509Certificates;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

    public int xCoord, yCoord;
    public int Width, Height;
    
    private DateTimeOffset _spawnTime;
    
    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        xCoord = position.X;
        yCoord = position.Y;

        Width = spriteSheet.FrameWidth;
        Height = spriteSheet.FrameHeight;

        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
    }
}