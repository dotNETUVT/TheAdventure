using Silk.NET.Maths;
using TheAdventure;

public class FlowerObject : GameObject
{
    /// <summary>
    /// Player X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Player Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

    public int WorldWidth { get; set; }

    public int WorldHeight { get; set; }

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 30, 30);
    private Rectangle<int> _target = new Rectangle<int>(0, 0, 30, 30);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    public FlowerObject( int id,int x, int y) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "flower.png"), out var textureData);
        X = x;
        Y = y;
       
        UpdateScreenTarget();
    }

    private void UpdateScreenTarget()
    {
        var targetX = X - 24;
        var targetY = Y - 24 + 6; //Y - 42;

        _target = new Rectangle<int>(targetX, targetY, 24, 24);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int time)
    {
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        X += (int)(right * pixelsToMove);
        X -= (int)(left * pixelsToMove);
        Y -= (int)(up * pixelsToMove);
        Y += (int)(down * pixelsToMove);

        if (X < 10) { X = 10; }
        if (Y < 24) { Y = 24; }
        if (X > WorldWidth - 10) { X = WorldWidth - 10; }
        if (Y > WorldHeight - 6) { Y = WorldHeight - 6; }

        UpdateScreenTarget();
    }

    public void Render(GameRenderer renderer)
    {
        renderer.RenderTexture(_textureId, _source, _target);
    }
}