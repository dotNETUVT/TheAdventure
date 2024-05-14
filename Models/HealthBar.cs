using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class HealthBar
    {
        private Rectangle<int> _position;
        private int _width;
        private int _height;
        private Texture _backgroundTexture;
        private Texture _foregroundTexture;
        private int v1;
        private int v2;
        private int v3;
        private int v4;
        private Color color1;
        private Color color2;

        public HealthBar(int x, int y, int width, int height, Texture backgroundTexture, Texture foregroundTexture)
        {
            _position = new Rectangle<int>(x, y, width, height);
            _width = width;
            _height = height;
            _backgroundTexture = backgroundTexture;
            _foregroundTexture = foregroundTexture;
        }

        public HealthBar(int v1, int v2, int v3, int v4, Color color1, Color color2)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
            this.color1 = color1;
            this.color2 = color2;
        }

    }
}
