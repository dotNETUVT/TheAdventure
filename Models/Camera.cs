using Silk.NET.Maths;


/// <summary>
/// Provides translation from world coordinates to screen coordinates.
/// </summary>
/// <remarks>
/// World coordinates are top = 0, left = 0, positivie in right and down direction.
/// </remarks>
public class Camera
{
    private int _x;
    private int _y;

    private Rectangle<int> _gameWorld = new();

    /// <summary>
    /// World coordinates.
    /// </summary>
    public int X
    {
        get { return _x; }
    }

    /// <summary>
    /// World coordinates.
    /// </summary>
    public int Y
    {
        get { return _y; }
    }

    public int Width { get; init; }
    public int Height { get; init; }

    public Camera(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void SetWorldBounds(Rectangle<int> bounds)
    {
        var marginLeft = Width / 2;
        var marginTop = Height / 2;

        if (marginLeft * 2 > bounds.Size.X)
        {
            marginLeft = 48;
        }

        if (marginTop * 2 > bounds.Size.Y)
        {
            marginTop = 48;
        }

        _gameWorld = new Rectangle<int>(marginLeft, marginTop, bounds.Size.X - marginLeft * 2,
            bounds.Size.Y - marginTop * 2);
        _x = marginLeft;
        _y = marginTop;
    }

    //public void LookAt(int x, int y)
    //{
    //    if (_gameWorld.Contains(new Vector2D<int>(_x, y)))
    //    {
    //        _y = y;
    //    }

    //    if (_gameWorld.Contains(new Vector2D<int>(x, _y)))
    //    {
    //        _x = x;
    //    }
    //}

    public int LookAt(int x1, int y1, int x2, int y2)
    {
        //Console.WriteLine("1: " + x1 + " " + y1 + " 2: " + x2 + " " + y2);
        int centerX = (x1 + x2) / 2;
        int centerY = (y1 + y2) / 2;
        // Opreste camera daca nu s-ar mai vedea decat un player
        if (y1 - y2 >= (Height - 40))
        {
            // player1 - nu se mai poate misca in jos
            // player2 - nu se mai poate misca in sus
            return 11;
        }
        if (y2 - y1 >= (Height - 40))
        {
            // player1 - nu se mai poate misca in sus
            // player2 - nu se mai poate misca in jos
            return 12;
        }
        if (x1 - x2 >= (Width - 40))
        {
            // player1 - nu se mai poate misca in dreapta
            // player2 - nu se mai poate misca in stanga
            return 22;
        }
        if (x2 - x1 >= (Width - 40))
        {
            // player1 - nu se mai poate misca in stanga
            // player2 - nu se mai poate misca in dreapta
            return 21;
        }

        // Mutam pozitia camerei daca totul e bine
        if (_gameWorld.Contains(new Vector2D<int>(_x, centerY)))
        {
            _y = centerY;
        }
        if(_gameWorld.Contains(new Vector2D<int>(centerX, _y)))
        {
            _x = centerX;
        }
        return 0;
    }

    /// <summary>
    /// Translates a rectangle from world coordinates to screen coordinates.
    ///
    /// Camera is always in the center of the screen.
    /// </summary>
    /// <param name="textureDestination"></param>
    /// <returns></returns>
    public Rectangle<int> TranslateToScreenCoordinates(Rectangle<int> textureDestination)
    {
        var newDestination = textureDestination.GetTranslated(new Vector2D<int>(Width / 2 - X, Height / 2 - Y));
        return newDestination;
    }

    public Vector2D<int> FromScreenToWorld(int x, int y)
    {
        return new Vector2D<int>(x - (Width / 2 - X), y - (Height / 2 - Y));
    }
}