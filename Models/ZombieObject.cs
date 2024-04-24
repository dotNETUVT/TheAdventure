using Silk.NET.Maths;
using TheAdventure;

namespace TheAdventure.Models;

public class ZombieObject : EnemyObject
{
    //private double _movementCooldown = 0.1; // Tiempo de espera entre cada movimiento del zombie

    public ZombieObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, x, y)
    {
        _movementCooldown = 0.10;
    }

}