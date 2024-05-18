using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class Prop : RenderableGameObject
    {
        public float Scale { get; set; } = 0.1f; // Scaling to 10% since the props are pretty big.
        public int TextureId { get; private set; } // Texture ID of the prop

        public Prop(GameRenderer renderer, string filePath, int x, int y) : base(null, (x, y))
        {
            LoadTexture(renderer, filePath);
        }

        private void LoadTexture(GameRenderer renderer, string filePath)
        {
            SpriteSheet = new SpriteSheet
            {
                FileName = filePath,
                FrameWidth = (int)renderer.GetTextureWidth(filePath), // Get texture width from renderer
                FrameHeight = (int)renderer.GetTextureHeight(filePath) // Get texture height from renderer
            };
            TextureId = renderer.LoadTexture(filePath, out _); // Load texture and get the ID
        }

        public bool CheckCollision(Rectangle<int> playerBounds)
        {
            var scaledWidth = (int)(SpriteSheet.FrameWidth * Scale); // Calculate scaled width
            var scaledHeight = (int)(SpriteSheet.FrameHeight * Scale); // Calculate scaled height
    
            var propBounds = new Rectangle<int>(Position.X, Position.Y, scaledWidth, scaledHeight); // Use scaled dimensions
            return Intersects(playerBounds, propBounds);
        }

        public void Render(GameRenderer renderer)
        {
            var destWidth = (int)(SpriteSheet.FrameWidth * Scale); // Calculate scaled width
            var destHeight = (int)(SpriteSheet.FrameHeight * Scale); // Calculate scaled height
            

            // Render the prop with the scaled destination rectangle
            renderer.RenderTexture(TextureId,
                new Rectangle<int>(0, 0, SpriteSheet.FrameWidth, SpriteSheet.FrameHeight),
                new Rectangle<int>(Position.X, Position.Y, destWidth, destHeight));
        }

        private bool Intersects(Rectangle<int> a, Rectangle<int> b)
        {
            return a.Origin.X < b.Origin.X + b.Size.X &&
                   a.Origin.X + a.Size.X > b.Origin.X &&
                   a.Origin.Y < b.Origin.Y + b.Size.Y &&
                   a.Origin.Y + a.Size.Y > b.Origin.Y;
        }
    }
}