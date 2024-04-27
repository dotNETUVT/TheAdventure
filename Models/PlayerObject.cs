using kbradu;
using Silk.NET.Maths;
using TheAdventure;

public class PlayerObject : GameObject
{
    /// <summary>
    /// Player X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Player Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

    public List<Coin> pocket { get; set; }

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    /// <summary>
    /// Does E key is pressed?
    /// </summary>
    public bool IsInteracting { get; set; } = false;

    public PlayerObject(int id) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
        pocket = new();
        UpdateScreenTarget();
    }

    private void UpdateScreenTarget(){
        var targetX = X + 24;
        var targetY = Y - 42;

        _target = new Rectangle<int>(targetX, targetY, 48, 48);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int time)
    {
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        X += (int)(right * pixelsToMove);
        X -= (int)(left * pixelsToMove);
        Y -= (int)(up * pixelsToMove);
        Y += (int)(down * pixelsToMove);

        UpdateScreenTarget();
    }

    string currentStatistics;
    public override void Update()
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        string currentStatistics = $"Player: Gold coins [{pocket.Count(x => x.Type == MaterialType.Gold)}] | Silver coins [{pocket.Count(x => x.Type == MaterialType.Silver)}]";
        Console.Write(currentStatistics);
    
    }
    public void Render(GameRenderer renderer){
        renderer.RenderTexture(_textureId, _source, _target);
    }
}