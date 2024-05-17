namespace TheAdventure.Models.Data;

public struct FramePosition{
    public int Row { get; set; }
    public int Col { get; set; }
}

public struct FrameOffset{
    private int v1;
    private int v2;

    public FrameOffset(int v1, int v2) : this()
    {
        this.v1 = v1;
        this.v2 = v2;
    }

    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}