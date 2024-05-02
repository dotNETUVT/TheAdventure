using Silk.NET.Maths;
using System;

public class Camera
{
    private int _x;
    private int _y;

    private Rectangle<int> _gameWorld = new();
    private Vector2D<int> _shakeOffset = new Vector2D<int>(0, 0);
    private int _shakeIntensity = 0;
    private float _shakeDuration = 0;

    public int X
    {
        get { return _x + _shakeOffset.X; } 
    }

    public int Y
    {
        get { return _y + _shakeOffset.Y; } 
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

    public void TriggerScreenShakeAfterDelay()
    {
        if (_shakeDuration > 0)
        {
            Random rnd = new Random();
            _shakeOffset.X = rnd.Next(-_shakeIntensity, _shakeIntensity);
            _shakeOffset.Y = rnd.Next(-_shakeIntensity, _shakeIntensity);
            _shakeDuration -= 0.016f; 
        }
        else
        {
            _shakeOffset = new Vector2D<int>(0, 0);
        }
    }

    public void TriggerScreenShake(float intensity, float duration)
    {
        _shakeIntensity = (int)Math.Round(intensity);
        _shakeDuration = duration;
    }

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
