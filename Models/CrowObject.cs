using Silk.NET.Maths;
using TheAdventure;

public class CrowObject : RenderableGameObject
{
    /// <summary>
    /// Player X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Player Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

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


    public CrowObject(string fileName, int durationInSeconds, int id, int numberOfFrames, int numberOfColumns, int numberOfRows, int x, int y) :
        base(fileName, id)
    {
        _durationInSeconds = durationInSeconds;
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _numberOfRows = numberOfRows;
        X=x; Y=y;

        _rowHeight = this.TextureInformation.Height / numberOfRows;
        _columnWidth = this.TextureInformation.Width / numberOfColumns;

        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;

        _timePerFrame = (durationInSeconds * 1000) / _numberOfFrames;

        this.TextureDestination = new Silk.NET.Maths.Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    }

    public override bool Update(int timeSinceLastFrame)
    {
        _timeSinceAnimationStart += timeSinceLastFrame;

        // Calculate the current frame
        var currentFrame = _timeSinceAnimationStart / _timePerFrame;

        // Check if animation has completed
        if (_timeSinceAnimationStart > _durationInSeconds * 1000)
        {
            // Reset animation
            _timeSinceAnimationStart = 0;
            currentFrame = 0;
        }

        if (X == 1200) return false;

        // Calculate current row and column
        _currentRow = currentFrame / _numberOfColumns;
        _currentColumn = currentFrame % _numberOfColumns;

        // Update X position (example: increase X by 10 pixels per frame)
        X += 10;

        // Update texture destination and source rectangles
        this.TextureDestination = new Silk.NET.Maths.Rectangle<int>(X, Y, _columnWidth, _rowHeight);
        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);

        return true;

        
    }


}