using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class SkeletonObject : EnemyObject
{

    public SkeletonObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {   
        _movementCooldown = 0.05;
    }

}