using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheAdventure.Models.FriendObject;
using static TheAdventure.Models.PlayerObject;

namespace TheAdventure.Models;

public class FriendObject : RenderableGameObject
{
    public enum FriendStateDirection
    {
        None = 0,
        Down,
        Up,
        Left,
        Right,
    }
    public enum FriendState
    {
        None = 0,
        Idle,
        Move
    }

    private int _pixelsPerSecond = 392;

    private string _currentAnimation = "IdleDown";

    public (FriendState State, FriendStateDirection Direction) State { get; private set; }

    public FriendObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SetState(FriendState.Idle, FriendStateDirection.Down);
    }

    public void SetState(FriendState state, FriendStateDirection direction)
    {
        if (State.State == state && State.Direction == direction)
        {
            return;
        }else if (state == FriendState.None && direction == FriendStateDirection.None)
        {
            SpriteSheet.ActivateAnimation(null);
        }
        else
        {
            var animationName = Enum.GetName<FriendState>(state) + Enum.GetName<FriendStateDirection>(direction);
            SpriteSheet.ActivateAnimation(animationName);
        }
        State = (state, direction);
    }

    public (int, int) SwitchFriendMovement(int initialX, int initialY)
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

    public void UpdateFriendPosition(double up, double down, double left, double right, (int X, int Y) playerPosition, int width, int height,
        double time, IEnumerable<TemporaryGameObject> gameObject, Dictionary<int, List<(int, int)>> layerCoordinates)
    {
        // Minimum distance the friend should maintain from player
        const int MinFollowDistance = 20; 

        var initialX = Position.X;
        var initialY = Position.Y;
        
        var directionX = playerPosition.X - Position.X;
        var directionY = playerPosition.Y - Position.Y;

        var distance = Math.Sqrt(Math.Pow(directionX, 2) + Math.Pow(directionY, 2));
        if (distance <= MinFollowDistance)
        {
            SetState(FriendState.Idle, State.Direction);
            return;
        }

        if (up <= double.Epsilon &&
            down <= double.Epsilon &&
            left <= double.Epsilon &&
            right <= double.Epsilon &&
            State.State == FriendState.Idle){
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

        // Set state based on the direction of movement
        if (y < Position.Y)
        {
            SetState(FriendState.Move, FriendStateDirection.Up);
        }
        if (y > Position.Y)
        {
            SetState(FriendState.Move, FriendStateDirection.Down);
        }
        if (x > Position.X)
        {
            SetState(FriendState.Move, FriendStateDirection.Right);
        }
        if (x < Position.X)
        {
            SetState(FriendState.Move, FriendStateDirection.Left);
        }
        if (x == Position.X && y == Position.Y)
        {
            SetState(FriendState.Idle, State.Direction);
        }

        Position = (x, y);

        foreach (var coordinates in layerCoordinates)
        {
            foreach (var coordTuple in coordinates.Value)
            {
                if (CollidesWithObject(coordTuple.Item1 * 16, coordTuple.Item2 * 16, 16, 16))
                {
                    Position = SwitchFriendMovement(initialX, initialY);
                }
            }
        }

        foreach (var obj in gameObject)
        {
            if (CollidesWithBomb(x, y, obj))
            {
                Position = SwitchFriendMovement(initialX, initialY);
            }
        }
    }

    // Function to prevent the Friend colliding with the bomb
    private bool CollidesWithBomb(int x, int y, TemporaryGameObject bomb)
    {
        int FriendRight = x + 4;

        int bombRight = bomb.Position.X + 16;
        int bombBottom = bomb.Position.Y + 12;

        return x - 4 < bombRight &&
                FriendRight > bomb.Position.X - 16 &&
                y < bombBottom &&
                y > bomb.Position.Y - 16;
    }

    // Function to prevent the Friend colliding with the tile objects
    public bool CollidesWithObject(int x, int y, int tileWidth, int tileHeight)
    {
        int FriendX = Position.X;
        int FriendY = Position.Y;

        return FriendX - 6 >= x - tileWidth / 2 &&
            FriendX - 8 <= x + tileWidth / 2 &&
            FriendY - 10 >= y - tileHeight / 2 &&
            FriendY - 22 <= y - tileHeight / 2;
    }

    // Method to get the bounding box of the Friend
    public System.Drawing.Rectangle GetBoundingBox()
    {
        // Assuming Friend's bounding box is a rectangle centered at its position with width and height of sprite
        return new System.Drawing.Rectangle((int)(Position.X - SpriteSheet.FrameWidth / 2), (int)(Position.Y - SpriteSheet.FrameHeight / 2), SpriteSheet.FrameWidth, SpriteSheet.FrameHeight);
    }
}

