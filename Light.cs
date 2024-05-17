using Silk.NET.Maths;

namespace TheAdventure
{
    public class LightSource
    {
        public Vector2D<int> Position { get; set; }
        public int Radius { get; set; }
        public LightColor Color { get; set; }

        public LightSource(Vector2D<int> position, int radius, LightColor color)
        {
            Position = position;
            Radius = radius;
            Color = color;
        }

        public void UpdatePosition(Vector2D<int> newPosition)
        {
            Position = newPosition;
        }
    }

    public struct LightColor(byte r, byte g, byte b, byte a)
    {
        public byte R { get; set; } = r;
        public byte G { get; set; } = g;
        public byte B { get; set; } = b;
        public byte A { get; set; } = a;
    }
}