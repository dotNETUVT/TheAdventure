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
    private float _zoom = 1.0f;

    // Camera shake variables
    private float shakeMagnitude = 0;
    private float shakeDuration = 0;
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

     public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Max(0.1f, Math.Min(5.0f, value)); 
    }

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

    public void LookAt(int x, int y)
    {
        if (_gameWorld.Contains(new Vector2D<int>(_x, y)))
        {
            _y = y;
        }

        if (_gameWorld.Contains(new Vector2D<int>(x, _y)))
        {
            _x = x;
        }
    }

    public void Update(float deltaTime)
    {
        if (shakeDuration > 0)
        {
            shakeDuration -= deltaTime;
            int shakeX = (int)(Random.Shared.NextDouble() * shakeMagnitude * 2 - shakeMagnitude);
            int shakeY = (int)(Random.Shared.NextDouble() * shakeMagnitude * 2 - shakeMagnitude);
            _x += shakeX;
            _y += shakeY;
        }
        else
        {
            shakeMagnitude = 0;
        }
    }

    public void Shake(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
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
        var scaledWidth = (int)(textureDestination.Size.X * _zoom);
        var scaledHeight = (int)(textureDestination.Size.Y * _zoom);
        var scaledX = (int)((textureDestination.Origin.X - _x) * _zoom + Width / 2);
        var scaledY = (int)((textureDestination.Origin.Y - _y) * _zoom + Height / 2);
        return new Rectangle<int>(new Vector2D<int>(scaledX, scaledY), new Vector2D<int>(scaledWidth, scaledHeight));
    }

    public Vector2D<int> FromScreenToWorld(int x, int y)
    {
        var worldX = (int)((x - Width / 2) / _zoom + _x);
        var worldY = (int)((y - Height / 2) / _zoom + _y);
        return new Vector2D<int>(worldX, worldY);
    }
}