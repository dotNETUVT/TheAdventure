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

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private int _textureId;
    private int _pixelsPerSecond = 128;

    private int _frameIndex;
    private double _animationTime;
    private const double FrameDuration = 0.1;
    private const int FrameCount = 6;

    private int FrameWidth;
    private int FrameHeight;
    
    private readonly int RowForDown = 3;
    private readonly int RowForRight = 4;
    private readonly int RowForUp = 5; 
    private readonly int RowForLeft = 6;

    public PlayerObject(int id) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
        UpdateScreenTarget();
        
        FrameWidth = textureData.Width / FrameCount;
        FrameHeight = textureData.Height / 11; // 11 rows in the sprite sheet because I added a row for the player moving left.
        _frameIndex = 0;
        _animationTime = 0;
        
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

        int animationRow = 0;
        if (up > 0)
        {
            animationRow = RowForUp;
        }
        else if (down > 0)
        {
            animationRow = RowForDown;
        }
        else if (left > 0)
        {
            animationRow = RowForLeft;
        }
        else if (right > 0)
        {
            animationRow = RowForRight;
        }
        
        if(up > 0 || down > 0 || left > 0 || right > 0){
            UpdateAnimation(animationRow, time);
        }

        UpdateScreenTarget();
    }
    
    private void UpdateAnimation(int row, int timeSinceLastUpdate){
        _animationTime += timeSinceLastUpdate / 1000.0;
        if(_animationTime >= FrameDuration){
            _frameIndex++;
            if(_frameIndex >= FrameCount){
                _frameIndex = 0;
            }
            _animationTime = 0;
        }

        _source = new Rectangle<int>(_frameIndex * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
    }

    public void Render(GameRenderer renderer){
        renderer.RenderTexture(_textureId, _source, _target);
    }
    
}