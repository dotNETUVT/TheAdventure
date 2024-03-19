using Silk.NET.Maths;

namespace TheAdventure.Models;

public class AnimatedGameObject : RenderableGameObject
{
    private int _durationInSeconds;
    private int _numberOfColumns;
    private int _numberOfRows;
    private int _numberOfFrames;
    private int _timeSinceAnimationStart = 0;

    private int _currentRow = 0;
    private int _currentColumn = 0;
    private int _rowHeight = 0;
    private int _columnWidth = 0;
    private int _timePerFrame;

    private DateTimeOffset _lastFrameAt;

    public AnimatedGameObject(GameRenderer renderer, string fileName, int durationInSeconds, int numberOfFrames,
        int numberOfColumns, int numberOfRows) :
        base(renderer, fileName)
    {
        _durationInSeconds = durationInSeconds;
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _numberOfRows = numberOfRows;
        _timePerFrame = (_durationInSeconds * 1000) / _numberOfFrames;
        _rowHeight = TextureInformation.Height / _numberOfRows;
        _columnWidth = TextureInformation.Width / _numberOfColumns;
        
        TextureSource = new Rectangle<int>(0, 0, _columnWidth, _rowHeight);
        TextureDestination = new Rectangle<int>(0, 0, _columnWidth * 2, _rowHeight * 2);
        
        _lastFrameAt = DateTimeOffset.UtcNow;
    }
    
    public override bool Update(int timeSinceLastFrame)
    {
        if ((int)DateTimeOffset.UtcNow.Subtract(_lastFrameAt).TotalMilliseconds >= _timePerFrame)
        {
            _lastFrameAt = DateTimeOffset.UtcNow;
            TextureSource = new Rectangle<int>(TextureSource.Origin.X + _columnWidth, 0, _columnWidth, _rowHeight);
        }
        
        if (TextureSource.Origin.X >= TextureInformation.Width)
        {
            return false;
        }
        
        return true;
    }
}