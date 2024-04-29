using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models.Data
{
    public class Object
    {
        public Vector2D<int> Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TextureId { get; set; }  // This will store the texture ID

        public Object(GameRenderer renderer, string texturePath, int x, int y, int width, int height)
        {
            Position = new Vector2D<int>(x, y);
            Width = width;
            Height = height;
            TextureId = renderer.LoadTexture(texturePath, out var textureInfo); // Load texture and store ID
                                                                                // Update the size according to the actual texture dimensions
            Width = textureInfo.Width;
            Height = textureInfo.Height;
        }

        public void Render(GameRenderer renderer)
        {
            if (TextureId >= 0) // Ensure the texture is loaded
            {
                var src = new Rectangle<int>(0, 0, Width, Height);
                var dst = new Rectangle<int>(Position.X, Position.Y, Width, Height);
                renderer.RenderTexture(TextureId, src, dst);
            }
        }
    }
}
