using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class RenderableGameObject : GameObject
{
    public SpriteSheet SpriteSheet { get; set; }
    public (int X, int Y) Position { get; set; }
    public double Angle { get; set; }
    public Point RotationCenter { get; set; }

    public RenderableGameObject(SpriteSheet spriteSheet, (int X, int Y) position, double angle = 0.0, Point rotationCenter = new())
        : base()
    {
        SpriteSheet = spriteSheet;
        Position = position;
        Angle = angle;
        RotationCenter = rotationCenter;
    }

    public virtual void Render(GameRenderer renderer)
    {
        SpriteSheet.Render(renderer, Position, Angle, RotationCenter);
    }
    
    public virtual string CollidesWith(RenderableGameObject other)
    {
        if (Position.X >= other.Position.X - other.SpriteSheet.FrameWidth / 2 &&
            Position.X <= other.Position.X + other.SpriteSheet.FrameWidth / 2 &&
            Position.Y >= other.Position.Y - other.SpriteSheet.FrameHeight / 2 &&
            Position.Y <= other.Position.Y + other.SpriteSheet.FrameHeight / 2 + 5
           )
        {
            if (Position.X < other.Position.X)
            {
                return "left";
            }
            if (Position.X > other.Position.X)
            {
                return "right";
            }
            if (Position.Y < other.Position.Y)
            {
                return "up";
            }
            if (Position.Y > other.Position.Y)
            {
                return "down";
            }
        }

        return "";
    }
}