public class Level
{
    public int Width { get; set; }
    public int Height { get; set; }
    public TileSetReference[] TileSets { get; set; } = Array.Empty<TileSetReference>();
    public Layer[] Layers { get; set; } = Array.Empty<Layer>();
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
}