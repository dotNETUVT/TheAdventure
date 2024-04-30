namespace TheAdventure
{
public class Level{
    public int Width { get; set; }
    public int Height{ get; set; }
    public required TileSetReference[] TileSets { get; set; }
    public required Layer[] Layers { get; set; }
    public int TileWidth {get;set;}
    public int TileHeight {get;set;}
}
}
