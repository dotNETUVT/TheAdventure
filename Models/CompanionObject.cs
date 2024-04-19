using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class CompanionObject : RenderableGameObject
{
    protected int _pixelsPerSecond = 192;
    protected bool _isWildAnimal = true;

    // idle = x=0, y=0
    // up = x=0, y=1
    // down = x=0, y=-1
    // left = x=-1, y=0
    // right = x=1, y=0
    protected int currentDirectionX = 0;
    protected int currentDirectionY = 0;


    public CompanionObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("Idle");
    }

    public bool IsWildAnimal()
    {
        return _isWildAnimal;
    }

    public void SetWildAnimal(bool wild)
    {
        _isWildAnimal = wild;
    }

    public virtual void SwitchAnimations(bool up, bool down, bool left, bool right)
    {
        if (_isWildAnimal)
            return;

        if (!left && !right && !up && !down)
        {
            SpriteSheet.ActivateAnimation("Idle");
            SetDirection(0, 0);
            return;
        }

        if(left && up){
            SpriteSheet.ActivateAnimation("LeftMov");
            SetDirection(-1, 0);
            return;
        }else if(left && down){
            SpriteSheet.ActivateAnimation("LeftMove");
            SetDirection(-1, 0);
            return;
        }else if(right && up){
            SpriteSheet.ActivateAnimation("RightMove");
            SetDirection(1, 0);
            return;
        }else if(right && down){
            SpriteSheet.ActivateAnimation("RightMove");
            SetDirection(1, 0);
            return;
        }

        if (left)
        {
            SpriteSheet.ActivateAnimation("LeftMove");
            SetDirection(-1, 0);
        }
        else if (right)
        {
            SpriteSheet.ActivateAnimation("RightMove");
            SetDirection(1, 0);
        }
        else if (up)
        {
            SpriteSheet.ActivateAnimation("UpMove");
            SetDirection(0, 1);
        }   
        else if (down)
        {
            SpriteSheet.ActivateAnimation("DownMove");
            SetDirection(0, -1);
        }
        else
        {
            SpriteSheet.ActivateAnimation("Idle");
            SetDirection(0, 0);
        }
    }

    public virtual void UpdateCompanionPosition(double up, double down, double left, double right, int width, int height,
        double time)
    {
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

        Position = (x, y);
    }

    public virtual void SetFollowingPosition(int x, int y)
    {   
        (int dirX, int dirY) = GetDirection();

        if (dirX == 0 && dirY == 0){
            return;
        } else if (dirX == 1 && dirY == 0){
            // right direcction movement
            Position = (x - 16, y - 14);
        } else if (dirX == -1 && dirY == 0){
            // left direcction movement
            Position = (x + 32, y - 14);
        } else if (dirX == 0 && dirY == 1){
            // up direcction movement
            Position = (x + 8, y + 16);
        } else if (dirX == 0 && dirY == -1){
            // down direcction movement
            Position = (x + 8 , y - 36);
        }

    }


    public virtual void SetDirection(int x, int y)
    {
        currentDirectionX = x;
        currentDirectionY = y;
    }

    public virtual (int, int) GetDirection()
    {
        return (currentDirectionX, currentDirectionY);
    }
}
