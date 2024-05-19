namespace TheAdventure.Models;

public struct TextureInfo
{
    public int Width { get; set; }
    public int Height { get; set; }

    public int PixelDataSize => Width * Height * 4;
}

