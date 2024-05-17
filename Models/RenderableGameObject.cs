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

    public virtual void Render(GameRenderer renderer, float scale = 1.0f)
{
    var scaledWidth = (int)(SpriteSheet.FrameWidth * scale);
    var scaledHeight = (int)(SpriteSheet.FrameHeight * scale);
    var destRect = new Rectangle<int>(
        Position.X - (int)(SpriteSheet.FrameCenter.OffsetX * scale),
        Position.Y - (int)(SpriteSheet.FrameCenter.OffsetY * scale),
        scaledWidth,
        scaledHeight
    );
    SpriteSheet.Render(renderer, destRect, Angle, RotationCenter);
}

}