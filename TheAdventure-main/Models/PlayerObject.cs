using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;

    public void DefineAnimations(SpriteSheet spriteSheet){
        spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 5),
                DurationMs = 1000,
                Loop = true
            };
        spriteSheet.Animations["IdleUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 5),
                DurationMs = 1000,
                Loop = true
            };
        spriteSheet.Animations["IdleRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 5),
                DurationMs = 1000,
                Loop = true
            };
        spriteSheet.Animations["WalkDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 5),
                DurationMs = 1000,
                Loop = true
            };
        spriteSheet.Animations["WalkUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (5, 0),
                EndFrame = (5, 5),
                DurationMs = 1000,
                Loop = true
            };
        spriteSheet.Animations["WalkRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (4, 0),
                EndFrame = (4, 5),
                DurationMs = 1000,
                Loop = true
            };
    }

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        DefineAnimations(spriteSheet);
        SpriteSheet.ActivateAnimation("IdleDown");
    }

    public void DecideWalkingAnimation(double up, double down, double left, double right){
        if(up == 0 && down == 0){
            if(left > 0)
                SpriteSheet.ActivateAnimation("WalkRight", true);
            if(right > 0)
                SpriteSheet.ActivateAnimation("WalkRight");
            }
        else{
            if(up > 0)
                SpriteSheet.ActivateAnimation("WalkUp");
            if(down > 0)
                SpriteSheet.ActivateAnimation("WalkDown");
            }
        if(up == 0 && down == 0 && right == 0 && left == 0){
            if(SpriteSheet.GetCurrentAnimation() == "WalkRight" && SpriteSheet.ActiveAnimation.Flip == Silk.NET.SDL.RendererFlip.Horizontal)
                SpriteSheet.ActivateAnimation("IdleRight", true);
            if(SpriteSheet.GetCurrentAnimation() == "WalkRight" && SpriteSheet.ActiveAnimation.Flip == Silk.NET.SDL.RendererFlip.None)
                SpriteSheet.ActivateAnimation("IdleRight");
            if(SpriteSheet.GetCurrentAnimation() == "WalkUp")
                SpriteSheet.ActivateAnimation("IdleUp");
            if(SpriteSheet.GetCurrentAnimation() == "WalkDown")
                SpriteSheet.ActivateAnimation("IdleDown");
            }
    }

    public void UpdatePlayerPosition(double dash, double up, double down, double left, double right, int width, int height,
        double time)
    {
        var pixelsToMove = time * _pixelsPerSecond;
        if(dash > 0)
            pixelsToMove *= 1.5;

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


        DecideWalkingAnimation(up, down, left, right);
        Position = (x, y);
    }
}