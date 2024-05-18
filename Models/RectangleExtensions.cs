namespace TheAdventure.Models;
using Silk.NET.Maths;

public static class RectangleExtensions
{
    public static bool Overlaps(this Rectangle<int> rect1, Rectangle<int> rect2)
    {
        return rect1.Origin.X < rect2.Origin.X + (rect2.Size.X)/2 &&
               rect1.Origin.X + (rect1.Size.X)/2 > rect2.Origin.X &&
               rect1.Origin.Y < rect2.Origin.Y + (rect2.Size.Y)/2 &&
               rect1.Origin.Y + (rect1.Size.Y)/2 > rect2.Origin.Y;
    }
}