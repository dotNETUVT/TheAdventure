using Silk.NET.SDL;

namespace TheAdventure.Models;

public class TemporaryGameObject : RenderableGameObject
{
    public double Ttl { get; init; }
    public bool IsExpired => (DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl;

    private DateTimeOffset _spawnTime;
    private double _speed; // Speed of the bomb
    private (double X, double Y) _position; // Maintain position in double precision

    public TemporaryGameObject(SpriteSheet spriteSheet, double ttl, (int X, int Y) position, double speed = 50.0, double angle = 0.0, Point rotationCenter = new())
        : base(spriteSheet, position, angle, rotationCenter)
    {
        Ttl = ttl;
        _spawnTime = DateTimeOffset.Now;
        _speed = speed;
        _position = (position.X, position.Y); // Initialize double precision position
    }

    public void UpdatePosition((int X, int Y) playerPosition, double deltaTime)
    {
        double directionX = playerPosition.X - _position.X;
        double directionY = playerPosition.Y - _position.Y;
        var length = Math.Sqrt(directionX * directionX + directionY * directionY);

        if (length > 0)
        {
            directionX /= length; // Normalize the direction vector
            directionY /= length;

            _position = (
                _position.X + (_speed * directionX * deltaTime),
                _position.Y + (_speed * directionY * deltaTime)
            );
        }

        // Update the RenderableGameObject Position to the nearest integer values
        Position = ((int)_position.X, (int)_position.Y);


    }
}
