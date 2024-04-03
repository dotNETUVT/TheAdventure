public class Tile
{
    public int Id { get; set; }
    public string Image { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }

    public int InternalTextureId { get; set; } = -1;

    public override string ToString()
    {
        return $"Id: {Id}, Image: {Image}, ImageWidth: {ImageWidth}, ImageHeight: {ImageHeight}, InternalTextureId: {InternalTextureId}";
    }
}