using Silk.NET.Maths;
using TheAdventure;

public class RenderableGameObject : GameObject
{
    public int TextureId{ get; set; }
    public int TextureRotation { get; set; }
    public Silk.NET.SDL.Point TextureRotationCenter{ get; set; }
    public Silk.NET.Maths.Rectangle<int> TextureSource { get; set; }
    public Silk.NET.Maths.Rectangle<int> TextureDestination { get; set; }

    public TextureData TextureInformation { get; set; }

    public RenderableGameObject(string fileName, int id):
        base(id)
    {
        LoadTexture(fileName);
    }
    protected virtual void LoadTexture(string fileName){
        TextureId = GameRenderer.LoadTexture(Path.Combine("Assets", fileName), out var textureData);
        TextureInformation = textureData;
        TextureSource = new Silk.NET.Maths.Rectangle<int>(0, 0, textureData.Width, textureData.Height);
        TextureDestination = new Silk.NET.Maths.Rectangle<int>(0, 0, textureData.Width, textureData.Height);
    }

    public virtual void Render(GameRenderer renderer){
        renderer.RenderGameObject(this);
    }

    public virtual bool Update(int timeSinceLastFrame){
        return true;
    }
}