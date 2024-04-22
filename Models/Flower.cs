using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models;

public class Flower : RenderableGameObject
{
    public Flower(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
      
    }


    public override void Render(GameRenderer renderer)
    {
     
        try
        { 
            base.Render(renderer);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rendering failed: {ex.Message}");
           
        }
    }

}