using Silk.NET.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models.MagicStones
{
    public class MagicStones
    {
        public List<MagicStoneObject> magicStoneObjects { get; } = new();
        static int currentId = 3500;


        public void addStone(int x, int y)
        {
            magicStoneObjects.Add(new MagicStoneObject(currentId, x, y));
            currentId++;
        }

        public void renderStones(GameRenderer renderer)
        {
            foreach (var stone in magicStoneObjects)
            {
                stone.Render(renderer);
            }
        }

        // verify if a stone already exist at that position
        public bool verifyPosition(int x, int y)
        {
            foreach (var stone in magicStoneObjects)
            {
                if (stone.X == x || stone.Y == y)
                    return false;
            }

            return true;
        }

    }

}
