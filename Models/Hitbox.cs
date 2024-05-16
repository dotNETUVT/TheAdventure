public class Hitbox
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int id { get; set; }
        public int x { get; set; }
        public int y { get; set; }

    public override string ToString()
    {
        return $"Hitbox ID: {id}, Width: {Width}, Height: {Height}, Position: ({x}, {y})";
    }
}
