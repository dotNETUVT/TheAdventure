using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public class WallObject : RenderableGameObject
    {
        public bool IsVertical { get; private set; }

        public WallObject(SpriteSheet spriteSheet, (int X, int Y) position, bool isVertical)
            : base(spriteSheet, position)
        {
            IsVertical = isVertical;
        }

        public static void AddWalls(SpriteSheet spriteSheet, Dictionary<int, GameObject> gameObjects, ref int currentId, (int X, int Y) startPoint, int horizontalCount, int verticalCount)
        {
            int spacing = 25;
            for (int i = 0; i < horizontalCount; i++)
            {
                gameObjects.Add(currentId++, new WallObject(spriteSheet, (startPoint.X + i * spacing, startPoint.Y), false));
            }

            for (int i = 0; i < verticalCount; i++)
            {
                gameObjects.Add(currentId++, new WallObject(spriteSheet, (startPoint.X, startPoint.Y + i * spacing), true));
            }
        }

        public static void InitializeWalls(GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
        {
            var wallSpriteSheet = SpriteSheet.LoadSpriteSheet("wall.json", "Assets", renderer);
            if (wallSpriteSheet == null) return;

            int currentIdHorizontal = 1100;
            int currentIdVertical = 1200;
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (80, 80), 4, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (70, 90), 0, 6);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (80, 230), 4, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (170, 90), 0, 2);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (170, 200), 0, 2);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (180, 130), 4, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (180, 190), 4, 0);

            //

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (280, 190), 2, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (280, 130), 2, 0);

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (310, 90), 0, 2);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (320, 80), 4, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (410, 90), 0, 6);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (320, 200), 0, 2);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (375, 230), 2, 0);

            // First Chest

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (370, 250), 0, 6);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (320, 250), 0, 8);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (330, 440), 10, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (375, 390), 8, 0);

            // Red Obstacle

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (560, 200), 0, 8);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (620, 150), 0, 10);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (430, 140), 8, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (425, 190), 6, 0);

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (630, 390), 3, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (690, 400), 0, 2);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (630, 440), 3, 0);

            // Yellow Chest

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (565, 460), 0, 6);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (620, 460), 0, 4);

            // Yellow Obstacle

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (580, 600), 12, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (630, 550), 8, 0);

            // Blue Obstacle

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (870, 460), 0, 6);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (820, 460), 0, 4);

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (780, 450), 2, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (890, 450), 2, 0);

            AddWalls(wallSpriteSheet, gameObjects, ref currentIdHorizontal, (780, 350), 6, 0);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (770, 360), 0, 4);
            AddWalls(wallSpriteSheet, gameObjects, ref currentIdVertical, (920, 360), 0, 4);

        }

        public void Render(GameRenderer renderer, double angle)
        {
            var frameWidth = this.SpriteSheet.FrameWidth;
            var frameHeight = this.SpriteSheet.FrameHeight;

            Point rotationCenter;
            if (IsVertical)
            {
                rotationCenter = new Point(frameWidth / 2, frameHeight / 2);
            }
            else
            {
                rotationCenter = new Point(this.SpriteSheet.FrameCenter.OffsetX, this.SpriteSheet.FrameCenter.OffsetY);
            }

            this.SpriteSheet.Render(renderer, Position, angle, rotationCenter);
        }
    }
}
