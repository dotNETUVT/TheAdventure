class Interractable(int x, int y, int Width, int Height)
{
    private readonly int x = x * 16;
    private readonly int y = y * 16;
    private readonly int Width = Width * 16;
    private readonly int Height = Height * 16;

    public bool IsObjectInteracted(int x, int y)
    {
        if (x >= this.x && x <= this.x + this.Width && y >= this.y && y <= this.y + this.Height)
        {
            return true;
        }
        return false;
    }
}
