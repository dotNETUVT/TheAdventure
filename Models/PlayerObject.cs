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

    public bool active { get; set; }

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    public PlayerObject(int id, int x=0, int y=0) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
        X = x;
        Y = y;
        UpdateScreenTarget();
    }

    private void UpdateScreenTarget(){
        var targetX = -18;
        var targetY = -24;
        if (X+24 >= -18 && Y-42 >= -24 && Y - 42 <= 600 && X + 24 <= 930) {
            targetX = X + 24;
            targetY = Y - 42;
        } else if(X+24 < -18 || Y-42 < -24) {
            if (X+24 < -18) {
                targetX = -18;
                X = -42;
            } else { targetX = X + 24; }
            if (Y-42 < 0) { 
                targetY = -24;
                Y = 18;
            } else { targetY = Y - 42; }
        } else if(Y - 42 > 600 || X + 24 > 930) {
            if (Y - 42 > 600){
                targetY = 600;
                Y = 642;
            } else { targetY = Y - 42; }
            if (X+ 24 > 930) {
                targetX = 930;
                X = 906;
            } else {targetX = X + 24; }
        }
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

    public void undoUpdate(double up, double down, double left, double right, int time)
    {
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        X -= (int)(right * pixelsToMove);
        X += (int)(left * pixelsToMove);
        Y += (int)(up * pixelsToMove);
        Y -= (int)(down * pixelsToMove);

        UpdateScreenTarget();
    }

    public void Render(GameRenderer renderer, int sourceX=0, int sourceY=0){
        _source = new Rectangle<int> (sourceX, sourceY, 48, 48);
        renderer.RenderTexture(_textureId, _source, _target);
    }
}