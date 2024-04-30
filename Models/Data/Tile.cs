namespace TheAdventure
{
public class Tile
{
    public int Id { get; set; }
    public required string Image { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }

    public bool Solid { get; set;}

    public int InternalTextureId { get; set; } = -1;
}
}
