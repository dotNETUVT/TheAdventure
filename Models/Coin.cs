using Silk.NET.Maths;
using TheAdventure.Models;

public class Coin : RenderableGameObject
{
    public int Value { get; private set; }
    public bool Collected { get; private set; }

    public Coin((int X, int Y) position, int value, SpriteSheet spriteSheet) : base(spriteSheet, position)
    {
        Position = position;
        Value = value;
        Collected = false;
    }

    public void Collect()
    {
        Collected = true;
    }
}