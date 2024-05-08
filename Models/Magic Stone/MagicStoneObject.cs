using System.Formats.Asn1;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure;

namespace TheAdventure.Models;

public class MagicStoneObject : RenderableGameObject
{
    public int Health { get; set; } = 100;
    public MagicStoneObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y)) { }

    public void takeHit()
    {
        if (Health > 0)
        {
            Health -= 4;
        }
    }
    public override void Render(GameRenderer renderer)
    {
        if (Health > 0)
        {
            base.Render(renderer);
            RenderHealthBar(renderer);
        }
    }

    private void RenderHealthBar(GameRenderer renderer)
    {
        var screenPosition = renderer.TranslateToScreenCoords(Position.X, Position.Y);

        int barWidth = (int)(SpriteSheet.FrameWidth * 0.6);
        int barHeight = 6;
        int barX = screenPosition.X - barWidth / 2 + 8;
        int barY = screenPosition.Y - 38;

        // Calculate the percentage of health remaining
        double healthPercentage = (double)Health / 100;
        int remainingWidth = (int)(barWidth * healthPercentage);

        // Render the health bar background
        renderer.RenderRectangle(barX, barY, barWidth, barHeight, new Color(60, 60, 60, 255));

        // Render the remaining health
        renderer.RenderRectangle(barX + 2, barY + 2, remainingWidth - 4, barHeight - 4, new Color(80, 240, 80, 255));
    }
}