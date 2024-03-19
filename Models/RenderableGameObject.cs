using Silk.NET.Maths;
using TheAdventure;

public class RenderableGameObject : GameObject
{
    public int TextureId { get; set; }
    public Rectangle<int> TextureSource { get; set; }
    public Rectangle<int> TextureDestination { get; set; }
    public GameRenderer.TextureInfo TextureInformation {get;set;}

    public RenderableGameObject(GameRenderer renderer, string fileName) {
        var textureId = renderer.LoadTexture(fileName, out var textureInfo);
        TextureId = textureId;
        TextureSource = Rectangle.FromLTRB(0, 0, textureInfo.Width, textureInfo.Height);
        TextureDestination = TextureSource;
        TextureInformation = textureInfo;
    }

}