using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class ZombieObject : EnemyObject
{

    public ZombieObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {
        _movementCooldown = 0.10;
    }

}