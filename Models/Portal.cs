using Silk.NET.Maths;
using TheAdventure;


namespace TheAdventure.Models;

    public class Portal : RenderableGameObject
    {
        private Portal? _OtherSide;

        public bool IsFirst { get; private set; } = false;
        public bool IsLinked { get; set;} = false;

    public Portal(SpriteSheet spriteSheet,int x, int y) : base(spriteSheet, (x, y))
    {
        spriteSheet.ActivateAnimation("Idle");
    }
    
    public void setPortalAsFirt(ref Portal x)
    {
        x.IsFirst = true;
    }

    public void LinkAnotherPortal(ref Portal P1, ref Portal P2)
    {
        P1._OtherSide = P2;
        P2._OtherSide = P1;
        P1.IsLinked = true;
        P2.IsLinked = true;
    }

}


