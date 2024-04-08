using Silk.NET.Maths;
using TheAdventure;

public class MagicStoneObject : GameObject
{
    /// <summary>
    /// Stone X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Stone Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

    public int health { get; set; } = 100;


    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0, 0, 48, 48);
    private int _textureId;

    public MagicStoneObject(int id, int x, int y) : base(id)
    {
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "magic_rock.png"), out var textureData);
        X = x;
        Y = y;
    }
    

    public void Render(GameRenderer renderer)
    {
        _target = new Rectangle<int>(X, Y, 48, 48);
        renderer.RenderTexture(_textureId, _source, _target);
    }
}