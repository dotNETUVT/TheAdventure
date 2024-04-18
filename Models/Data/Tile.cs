public class Tile 
{
    public int Id { get; set; }
    public string Image { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }

    public int InternalTextureId { get; set; } = -1;
}