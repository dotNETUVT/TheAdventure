using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    private string _currentAnimation = "IdleDown";


    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation(_currentAnimation);
       
    }

    public (int, int) SwitchPlayerMovement(int initialX, int initialY)
    {
        switch (_currentAnimation)
        {
            case "MoveUp":
                initialY += 1;
                break;

            case "MoveDown":
                initialY -= 1;
                break;

            case "MoveLeft":
                initialX += 1;
                break;

            case "MoveRight":
                initialX -= 1;
                break;
        }

        return (initialX, initialY);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
        double time, IEnumerable<TemporaryGameObject> gameObject, Dictionary<int, List<(int, int)>> layerCoordinates)
    {
        var initialX = Position.X;
        var initialY = Position.Y;

        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            _currentAnimation == "IdleDown"){
            return;
        }

        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        if (x < 10)
        {
            x = 10;
        }

        if (y < 24)
        {
            y = 24;
        }

        if (x > width - 10)
        {
            x = width - 10;
        }

        if (y > height - 6)
        {
            y = height - 6;
        }

        if (y < Position.Y && _currentAnimation != "MoveUp"){
            _currentAnimation = "MoveUp";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (y > Position.Y && _currentAnimation != "MoveDown"){
            _currentAnimation = "MoveDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x > Position.X && _currentAnimation != "MoveRight"){
            _currentAnimation = "MoveRight";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x < Position.X && _currentAnimation != "MoveLeft"){
            _currentAnimation = "MoveLeft";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }
        if (x == Position.X && _currentAnimation != "IdleDown" &&
            y == Position.Y && _currentAnimation != "IdleDown"){
            _currentAnimation = "IdleDown";
            //Console.WriteLine($"Attempt to switch to {_currentAnimation}");
        }

        //Console.WriteLine($"Will to switch to {_currentAnimation}");
        SpriteSheet.ActivateAnimation(_currentAnimation);
        Position = (x, y);

        foreach (var coordinates in layerCoordinates)
            foreach (var coordTouple in coordinates.Value)
                if (CollidesWithObject(coordTouple.Item1 * 16, coordTouple.Item2 * 16, 16, 16))
                    // Console.WriteLine(coordTouple.Item1.ToString());
                    // Check curent animation to see where the player is moving
                    Position = SwitchPlayerMovement(initialX, initialY);

        foreach (var obj in gameObject)
            // Calling function for bomb collision
            if (CollidesWithBomb(x, y, obj))
                Position = SwitchPlayerMovement(initialX, initialY);
    }

    // Function to prevent the player colliding with the bomb
    private bool CollidesWithBomb(int x, int y, TemporaryGameObject bomb)
    {
        int playerRight = x + 4;

        int bombRight = bomb.Position.X + 16;
        int bombBottom = bomb.Position.Y + 12;

        return x - 4 < bombRight &&
               playerRight > bomb.Position.X - 16 &&
               y < bombBottom &&
               y > bomb.Position.Y - 16;
    }

    // Function to prevent the player colliding with the tile objects
    public bool CollidesWithObject(int x, int y, int tileWidth, int tileHeight)
    {
        int playerX = Position.X;
        int playerY = Position.Y;

        return playerX - 6 >= x - tileWidth / 2 &&
            playerX - 8 <= x + tileWidth / 2 &&
            playerY - 10 >= y - tileHeight / 2 &&
            playerY - 22 <= y - tileHeight / 2;
    }

    // Method to get the bounding box of the player
    public System.Drawing.Rectangle GetBoundingBox()
    {
        // Assuming player's bounding box is a rectangle centered at its position with width and height of sprite
        return new System.Drawing.Rectangle((int)(Position.X - SpriteSheet.FrameWidth / 2), (int)(Position.Y - SpriteSheet.FrameHeight / 2), SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
    }
}