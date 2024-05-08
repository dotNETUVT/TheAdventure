namespace TheAdventure
{
public class TileSet
{
    public required string Name { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int TileCount { get; set; }
    public required Tile[] Tiles { get; set; }
}

public class TileSetReference
{
    public string? Source { get; set; }
    public TileSet? Set { get; set; }
}
}