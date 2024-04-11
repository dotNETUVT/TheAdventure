using Silk.NET.Maths;
using TheAdventure;
using TheAdventure.Models.Data;

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

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    public PlayerObject(int id) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
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

    public bool IsCloseToBomb(Bomb bomb)
    {
        if (!bomb.HasExploded)
        {
            return false;
        }
        
        var BombX = bomb.TextureDestination.Origin.X;
        var BombY = bomb.TextureDestination.Origin.Y;
        var BombWidth = bomb.TextureInformation.Width;
        var BombHeight = bomb.TextureInformation.Height;

        
        if ((X + 48 > BombX && X + 48 < BombX + BombWidth) && (Y - 48 > BombY && Y - 48 < BombY + BombHeight))
        {
            return true;
        }
        return false;
    }

    public void Respawn()
    {
        X = 0;
        Y = 0;
        UpdateScreenTarget();
    }

    public void Render(GameRenderer renderer){
        renderer.RenderTexture(_textureId, _source, _target);
    }
}