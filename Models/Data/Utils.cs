namespace TheAdventure.Models.Data;

public struct FramePosition{
    public int Row { get; set; }
    public int Col { get; set; }
}

public struct FrameOffset(int offsetX, int offsetY)
{
    public int OffsetX { get; set; } = offsetX;
    public int OffsetY { get; set; } = offsetY;
}