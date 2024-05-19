using System.Numerics;

namespace TheAdventure.Models.Data;


public class Tree : RenderableGameObject
{
    public Tree(SpriteSheet spriteSheet, (int x, int y) position) : base(spriteSheet, position)
    {
    }
}
