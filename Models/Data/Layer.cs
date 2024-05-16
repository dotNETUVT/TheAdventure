using System.Text;

public class Layer
{
    public string name { get; set; }
    public int[] Data { get; set; }
    public int Height { get; set; }
    public int Width {get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; }
    public int Id { get; set; }

    public Hitbox[]? objects { get; set; } // called objects in Tiled -> represents hitboxes here

    public override string ToString()
    {
        if (objects != null && objects.Length > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Id: {Id}, Name: {name}, Height: {Height}, Width: {Width}, X: {X}, Y: {Y}, Visible: {Visible}\n");
            sb.Append("Objects:\n");
            foreach (var obj in objects)
            {
                sb.AppendLine(obj.ToString());
            }
            return sb.ToString();
        }
        else
        {
            return $"Id: {Id}, Name: {name}, Height: {Height}, Width: {Width}, X: {X}, Y: {Y}, Visible: {Visible}\nObjects: None";
        }
    }

}