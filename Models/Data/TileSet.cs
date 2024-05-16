public class TileSet
{
    public string Name { get; set; } = string.Empty;
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int TileCount { get; set; }
    public Tile[] Tiles { get; set; } = Array.Empty<Tile>();
}

public class TileSetReference
{
    public string Source { get; set; } = string.Empty;
    public TileSet Set { get; set; } = new TileSet();
}