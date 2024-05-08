using System.Formats.Asn1;
using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class MagicStoneObject : RenderableGameObject
{
    public int health { get; set; } = 100;
    public MagicStoneObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y)) { }
}