using Microsoft.VisualBasic;
using TheAdventure;

public class AnimatedGameObject : RenderableGameObject
{
    private int _numberOfColumns;
    private int _timeSinceAnimationStart = 0;

    private int _currentRow = 0;
    private int _currentColumn = 0;
    private int _rowHeight = 0;
    private int _columnWidth = 0;

    private int _timePerFrame;
    private bool _isRepeating;

    public AnimatedGameObject(string fileName, int id, int numberOfFrames, int x, int y):
        base(fileName, id){
        _rowHeight = this.TextureInformation.Height;
        _columnWidth = this.TextureInformation.Width / numberOfFrames;
        _timePerFrame = 1000 / numberOfFrames;
        _numberOfColumns = numberOfFrames;

        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;

        this.TextureDestination = new Silk.NET.Maths.Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    }
    protected void ChangeAnimation(string fileName, int id, int numberOfFrames, int x, int y)
    {
        LoadTexture(fileName);
        _rowHeight = this.TextureInformation.Height;
        _columnWidth = this.TextureInformation.Width / numberOfFrames;
        _timePerFrame = 1000 / numberOfFrames;
        _numberOfColumns = numberOfFrames;

        var halfRow = _rowHeight / 2;
        var halfColumn = _columnWidth / 2;

        this.TextureDestination = new Silk.NET.Maths.Rectangle<int>(x - halfColumn, y - halfRow, _columnWidth, _rowHeight);
        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    }

    protected override void LoadTexture(string fileName)
    {
        TextureId = GameRenderer.LoadTexture(Path.Combine("Assets/Animations", fileName), out var textureData);
        TextureInformation = textureData;
        TextureSource = new Silk.NET.Maths.Rectangle<int>(0, 0, textureData.Width, textureData.Height);
        TextureDestination = new Silk.NET.Maths.Rectangle<int>(0, 0, textureData.Width, textureData.Height);
    }
    public void setAnimationSpeed(int timePerFrameInMilliseconds) {
        _timePerFrame = timePerFrameInMilliseconds;
    }

    public void startAnimationLoop()
    {
        _isRepeating = true;
    }

    public void stopAnimationLoop()
    {
        _isRepeating = false;
    }

    public override bool Update(int timeSinceLastFrame){
        

        _timeSinceAnimationStart += timeSinceLastFrame;

        var currentFrame = _timeSinceAnimationStart / _timePerFrame;

        if (_timeSinceAnimationStart > _timePerFrame * _numberOfColumns)
        {
            if (!_isRepeating) return false;

            currentFrame = 0;
            _timeSinceAnimationStart = 0;
        }
        _currentRow = currentFrame / _numberOfColumns;
        _currentColumn = currentFrame % _numberOfColumns; //- (_currentRow * _numberOfColumns);

        //Console.WriteLine($"{this.Id}: currentFrame: {currentFrame} currentRow: {_currentRow} currentColumn: {_currentColumn}");

        this.TextureSource = new Silk.NET.Maths.Rectangle<int>(_currentColumn * _columnWidth, _currentRow * _rowHeight, _columnWidth, _rowHeight);
    
        return true;
    }

    
}