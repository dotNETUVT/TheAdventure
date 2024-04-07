using kbradu;
public class Level{
    public int Width { get; set; }
    public int Height{ get; set; }
    public TileSetReference[] TileSets { get; set; }
    public Layer[] Layers { get; set; }
    public ChestObject[] ChestSets { get; set; }
}