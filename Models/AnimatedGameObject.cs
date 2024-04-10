using Microsoft.VisualBasic;
using Silk.NET.Maths;
using TheAdventure;

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

    public AnimatedGameObject(string fileName, int durationInSeconds, int id, int numberOfFrames, int numberOfColumns,int numberOfRows, int x, int y):
        base(fileName, id){
        _durationInSeconds = durationInSeconds;
        _numberOfFrames = numberOfFrames;
        _numberOfColumns = numberOfColumns;
        _numberOfRows = numberOfRows;

        _rowHeight = this.TextureInformation.Height / numberOfRows;
        _columnWidth = this.TextureInformation.Width / numberOfColumns;

        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;

        _timePerFrame = (durationInSeconds * 1000) / _numberOfFrames;

        this.TextureDestination = new Silk.NET.Maths.Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    }

    //offset if needed for objects (eg. bomb)
    public void updateOffset(int offsetX, int offsetY)
    {
        Silk.NET.Maths.Vector2D<int> translationVector = new Silk.NET.Maths.Vector2D<int>(offsetX, offsetY);
        this.TextureDestination = this.TextureDestination.GetTranslated(translationVector);
    }

    public override bool Update(int timeSinceLastFrame){
        

        _timeSinceAnimationStart += timeSinceLastFrame;

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;

        if (_timeSinceAnimationStart > _durationInSeconds * 1000) return false;

        _currentRow = currentFrame / _numberOfColumns;
        _currentColumn = currentFrame % _numberOfColumns; //- (_currentRow * _numberOfColumns);

        //Console.WriteLine($"{this.Id}: currentFrame: {currentFrame} currentRow: {_currentRow} currentColumn: {_currentColumn}");

        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    
        return true;
    }

    
}