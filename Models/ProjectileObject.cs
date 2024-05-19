using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure;
using TheAdventure.Models;

public class ProjectileObject : TemporaryGameObject
{
    private int _textureId;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double Speed { get; private set; }
    public (double X, double Y) Direction { get; private set; }

    public ProjectileObject(int textureId, int width, int height, double ttl, (int X, int Y) position, (double X, double Y) direction, double speed)
    : base(null, ttl, position)
    {
        _textureId = textureId;
        Width = width;
        Height = height;
        Direction = direction;
        Speed = speed;
    }

    public void Update(double deltaTime)
    {
        // Update position based on direction and speed
        Position = (
            (int)(Position.X + Direction.X * Speed * deltaTime),
            (int)(Position.Y + Direction.Y * Speed * deltaTime)
        );
    }


    public override void Render(GameRenderer renderer)
    {
        var dst = new Rectangle<int>(Position.X, Position.Y, Width, Height);
        var src = new Rectangle<int>(0, 0, Width, Height); // Assuming we want to render the entire texture
        renderer.RenderTexture(_textureId, src, dst);
    }
}
