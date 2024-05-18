using System;
using TheAdventure.Models;
using Silk.NET.Maths;
using TheAdventure;

public class EnemyObject : RenderableGameObject
{
    private static readonly Random random = new Random();
    private double _movementCooldown = 2.0;
    private DateTimeOffset _lastMoved = DateTimeOffset.Now;
    public bool IsDead { get; private set; } = false;

    public EnemyObject(SpriteSheet spriteSheet, int x, int y) : base(new SpriteSheet(spriteSheet), (x, y))
    {
    }

    public void UpdatePosition(Level level, Dictionary<int, GameObject> gameObjects, GameRenderer renderer)
    {
        if ((DateTimeOffset.Now - _lastMoved).TotalSeconds >= _movementCooldown)
        {
            var direction = random.Next(0, 4);
            MoveInDirection(direction, level);
            PlaceBombIfNeeded(renderer, gameObjects);

            _lastMoved = DateTimeOffset.Now;
        }
    }

    private void MoveInDirection(int direction, Level level)
    {
        int tileWidth = level.TileWidth;
        int tileHeight = level.TileHeight;
        int newX = Position.X, newY = Position.Y;

        switch (direction)
        {
            case 0: newY -= tileHeight; break;
            case 1: newY += tileHeight; break;
            case 2: newX -= tileWidth; break;
            case 3: newX += tileWidth; break;
        }

        if (!CheckCollision(newX, newY, level))
        {
            Position = (newX, newY);
        }
    }

    private bool CheckCollision(int x, int y, Level level)
    {
        int tileX = x / level.TileWidth;
        int tileY = y / level.TileHeight;
        return level.Layers[0].Data[tileY * level.Width + tileX] == 1;
    }

    private void PlaceBombIfNeeded(GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
    {
        if (random.NextDouble() > 0.5)
        {
            AddBomb(Position.X, Position.Y, renderer, gameObjects);
        }
    }

    private void AddBomb(int x, int y, GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
    {
        var spriteSheet = SpriteSheet.LoadSpriteSheet("bomb.json", "Assets", renderer);
        if (spriteSheet != null)
        {
            spriteSheet.ActivateAnimation("Explode");
            var bomb = new TemporaryGameObject(spriteSheet, 2.1, (x, y), bomb => HandleExplosion(bomb, gameObjects));
            gameObjects.Add(bomb.Id, bomb);
        }
    }

    private void HandleExplosion(TemporaryGameObject bomb, Dictionary<int, GameObject> gameObjects)
    {
        // Intentionally left blank
    }

    public void Die()
    {
        SpriteSheet.ActivateAnimation("Die");
        IsDead = true;
    }

    public void MarkForRemoval()
    {
        Die();
    }

}
