public class Layer
{
    public int[] Data { get; set; }
    public int Height { get; set; }
    public int Width {get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool Visible { get; set; }
    public int Id { get; set; }
    public float Opacity { get; set; }

    public override string ToString()
    {
        return $"Id: {Id}, Opacity: {Opacity}, Height: {Height}, Width: {Width}, X: {X}, Y: {Y}, Visible: {Visible}";
    }
}