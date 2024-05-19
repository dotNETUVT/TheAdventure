using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models.Data
{
    public class Boulder : RenderableGameObject
    {
        public Boulder(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
        }
    }
}
