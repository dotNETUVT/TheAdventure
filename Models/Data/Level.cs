public class Level{
    public int Width { get; set; }
    public int Height{ get; set; }
    public TileSetReference[] TileSets { get; set; }
    public Layer[] Layers { get; set; }
    public int TileWidth {get;set;}
    public int TileHeight {get;set;}

    public int TotalWidth => Width * TileWidth;
    public int TotalHeight => Height * TileHeight;
}
