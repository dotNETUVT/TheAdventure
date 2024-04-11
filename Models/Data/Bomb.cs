namespace TheAdventure.Models.Data;

public class Bomb : AnimatedGameObject
{
    public bool HasExploded { get; private set; }
    public Bomb(string fileName, int durationInSeconds, int id, int numberOfFrames, int numberOfColumns, int numberOfRows, int x, int y) : 
        base(fileName, durationInSeconds, id, numberOfFrames, numberOfColumns, numberOfRows, x, y)
    {
        HasExploded = false;
    }
    
    public override bool Update(int timeSinceLastFrame)
    {
        _timeSinceAnimationStart += timeSinceLastFrame;

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;

        if (_timeSinceAnimationStart > _durationInSeconds * 1000 && !HasExploded)
        {
            HasExploded = true;
            return true;
        }
        else if (_timeSinceAnimationStart > _durationInSeconds * 1000 && HasExploded)
        {
            return false;
        }

        _currentRow = currentFrame / _numberOfColumns;
        _currentColumn = currentFrame % _numberOfColumns; //- (_currentRow * _numberOfColumns);

        //Console.WriteLine($"{this.Id}: currentFrame: {currentFrame} currentRow: {_currentRow} currentColumn: {_currentColumn}");

        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    
        return true;
    }
}