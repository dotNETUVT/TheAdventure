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

    public bool CheckCollision(Rectangle<int> boundingBox, Rectangle<int> target)
    {
        if (boundingBox.Origin.X < target.Origin.X + target.Size.X &&
            boundingBox.Origin.X + boundingBox.Size.X > target.Origin.X &&
            boundingBox.Origin.Y + boundingBox.Size.Y > target.Origin.Y &&
            boundingBox.Origin.Y < target.Origin.Y + target.Size.Y)
        {
            return true;
        }

        return false;
    }

    public virtual bool CheckCollision(Rectangle<int> target)
    {
        var boundingBox = GetBoundingBox();
        if (boundingBox.Origin.X < target.Origin.X + target.Size.X &&
            boundingBox.Origin.X + boundingBox.Size.X > target.Origin.X &&
            boundingBox.Origin.Y + boundingBox.Size.Y > target.Origin.Y &&
            boundingBox.Origin.Y < target.Origin.Y + target.Size.Y)
        {
            return true;
        }

        return false;
    }

    public virtual List<PlayerObject.PlayerStateDirection> PredictCollision(Rectangle<int> target)
    {
        List<PlayerObject.PlayerStateDirection> directions = [];

        var upCollision = GetBoundingBox();
        upCollision.Origin.Y -= 5;
        var downCollision = GetBoundingBox();
        downCollision.Origin.Y += 5;
        var leftCollision = GetBoundingBox();
        leftCollision.Origin.X -= 5;
        var rightCollision = GetBoundingBox();
        rightCollision.Origin.X += 5;

        if (CheckCollision(upCollision, target))
        {
            directions.Add(PlayerObject.PlayerStateDirection.Up);
        }

        if (CheckCollision(downCollision, target))
        {
            directions.Add(PlayerObject.PlayerStateDirection.Down);
        }

        if (CheckCollision(leftCollision, target))
        {
            directions.Add(PlayerObject.PlayerStateDirection.Left);
        }

        if (CheckCollision(rightCollision, target))
        {
            directions.Add(PlayerObject.PlayerStateDirection.Right);
        }

        if (directions.Count == 0)
        {
            directions.Add(PlayerObject.PlayerStateDirection.None);
        }

        return directions;
    }

    public virtual Vector2D<int> GetCollisionOffset(Rectangle<int> target, PlayerObject.PlayerStateDirection direction)
    {
        var offset = new Vector2D<int>(0, 0);

        var boundingBox = GetBoundingBox();

        int thisLeftEdge = boundingBox.Origin.X;
        int thisRightEdge = boundingBox.Origin.X + boundingBox.Size.X;
        int thisUpEdge = boundingBox.Origin.Y;
        int thisDownEdge = boundingBox.Origin.Y + boundingBox.Size.Y;

        int targetLeftEdge = target.Origin.X;
        int targetRightEdge = target.Origin.X + target.Size.X;
        int targetUpEdge = target.Origin.Y;
        int targetDownEdge = target.Origin.Y + target.Size.Y;

        if (direction == PlayerObject.PlayerStateDirection.Down)
        {
            offset.Y = targetUpEdge - thisDownEdge;
        }
        else if (direction == PlayerObject.PlayerStateDirection.Up)
        {
            offset.Y = targetDownEdge - thisUpEdge;
        }
        else if (direction == PlayerObject.PlayerStateDirection.Left)
        {
            offset.X = targetRightEdge - thisLeftEdge;
        }
        else if (direction == PlayerObject.PlayerStateDirection.Right)
        {
            offset.X = targetLeftEdge - thisRightEdge;
        }
        
        return offset;
    }

    public virtual Rectangle<int> GetBoundingBox()
    {
        var frameCenter = SpriteSheet.FrameCenter;
        var frameWidth = SpriteSheet.FrameWidth;
        var frameHeight = SpriteSheet.FrameHeight;
        var scaleX = SpriteSheet.ScaleX;
        var scaleY = SpriteSheet.ScaleY;

        return new Rectangle<int>(Position.X - frameCenter.OffsetX, Position.Y - frameCenter.OffsetY,
            (int)(frameWidth * scaleX), (int)(frameHeight * scaleY));
    }
}