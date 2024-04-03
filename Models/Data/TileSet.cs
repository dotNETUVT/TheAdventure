public class TileSet
{
    public string Name { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int TileCount { get; set; }
    public Tile[] Tiles { get; set; }

    public override string ToString()
    {
        return $"Name: {Name}, TileWidth: {TileWidth}, TileHeight: {TileHeight}, TileCount: {TileCount}";
    }
}

public class TileSetReference
{
    public string Source { get; set; }
    public TileSet Set { get; set; }
    public int Firstgid { get; set; }

    public override string ToString()
    {
        return $"Source: {Source}, TileSet: {Set}, fGID: {Firstgid}";
    }
}