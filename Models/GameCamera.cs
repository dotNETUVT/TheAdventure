using Silk.NET.Maths;


/// <summary>
/// Provides translation from world coordinates to screen coordinates.
/// </summary>
/// <remarks>
/// World coordinates are top = 0, left = 0, positivie in right and down direction.
/// </remarks>
public class GameCamera
{
    /// <summary>
    /// World coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// World coordinates.
    /// </summary>
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle<int> TranslateToScreenCoordinates(Rectangle<int> textureDestination)
    {
        // In screen coordinates x = width / 2, y = height / 2.
        var vector = new Vector2D<int>(0, 0);
        if (X + 24 >= Width - 24)
        {
            vector.X = Width - 54  - X;
        } 
        var newDestination = textureDestination.GetTranslated(vector);
        return newDestination;
    }
}