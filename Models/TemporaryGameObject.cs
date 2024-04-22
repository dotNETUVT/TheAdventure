using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;
    
    private DateTimeOffset _spawnTime;
    
    private SoundPlayer _appearSoundPlayer;
    
    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position,string soundFilePath, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
        _appearSoundPlayer = new SoundPlayer("Assets/Explosion.wav");
        _appearSoundPlayer.Play();
        
    }
}