using System.Text;

public class Level{
    public int Width { get; set; }
    public int Height{ get; set; }
    public TileSetReference[] TileSets { get; set; }
    public Layer[] Layers { get; set; }
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Width: {Width}, Height: {Height}");
        stringBuilder.AppendLine("TileSets:");
        foreach (var tileSet in TileSets)
        {
            stringBuilder.AppendLine(tileSet.ToString());
        }
        stringBuilder.AppendLine("Layers:");
        foreach (var layer in Layers)
        {
            stringBuilder.AppendLine(layer.ToString());
        }
        return stringBuilder.ToString();
    }
}