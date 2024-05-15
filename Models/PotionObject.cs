using TheAdventure.Models;

public class PotionObject : RenderableGameObject
{
    public PotionType Type { get; private set; }
    public int Value { get; private set; }

    public PotionObject(SpriteSheet spriteSheet, int x, int y, PotionType type, int value) : base(spriteSheet, (x, y))
    {
        Type = type;
        Value = value;
    }
}

public enum PotionType
{
    Health,
    Attack
}