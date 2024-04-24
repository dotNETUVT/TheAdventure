using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class SkeletonObject : EnemyObject
{
    //private double _movementCooldown = 0.06; // Tiempo de espera entre cada movimiento del zombie


    public SkeletonObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {
        _movementCooldown = 0.05;
    }

}