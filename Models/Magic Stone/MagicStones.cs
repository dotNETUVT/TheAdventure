using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models;
public class MagicStones
{
    public List<MagicStoneObject> magicStoneObjects { get; } = new();

    public void addStone(SpriteSheet spriteSheet ,int x, int y)
    {
        magicStoneObjects.Add(new MagicStoneObject(spriteSheet, x, y));
    }

    public void renderStones(GameRenderer renderer)
    {
        foreach (var stone in magicStoneObjects)
        {
            stone.Render(renderer);
        }
    }

    public bool verifyPosition(int x, int y)
    {
        foreach (var stone in magicStoneObjects)
        {
            if (stone.Position.X == x || stone.Position.Y == y)
                return false;
        }

        return true;
    }
}
