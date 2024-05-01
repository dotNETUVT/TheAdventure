using Silk.NET.Maths;
using TheAdventure;

public  class AppleObject 
{
    private Rectangle<int> _source = new Rectangle<int>(0, 0, 20 , 20);
    private Rectangle<int> _target;
    private int _textureId;

    private readonly int minXOffset = 0;
    private readonly int maxXOffset = 600;  
    private readonly int minYOffset = 0; 
    private readonly int maxYOffset = 600;
    public int X { get; set; }
    public int Y { get; set; }
    private Random random = new Random();

    public AppleObject()
    {
        _textureId = GameRenderer.LoadTexture("Apple.png", out var textureData);
        Update();
    }


    public Rectangle<int> GetHitbox()
    {
        return _target;
    }
    public void Render(GameRenderer renderer)
    {
        renderer.RenderTexture(_textureId, _source, _target);
    }
    public void Update()
    {
        random = new Random();
        int randomXOffset = random.Next(minXOffset, maxXOffset + 1);
        int randomYOffset = random.Next(minYOffset, maxYOffset + 1);
        X= randomXOffset;
        Y = randomYOffset;
        _target = new Rectangle<int>(X, Y, 20, 20);
    }

    public bool IntersectsWith( PlayerObject player)
    {
        // Check if one rectangle is on left side of other
        if (player.X + 20 < X || X + 20 < player.X)
            return false;

        // Check if one rectangle is above other
        if (player.Y + 20 < Y || Y + 20 < player.Y)
            return false;

        // Rectangles overlap
        return true;
    }


}
