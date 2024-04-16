namespace TheAdventure.Models;

public class PlayerObject : RenderableGameObject
{
    private int _pixelsPerSecond = 192;
    private string _lastDirection = "IdleDown"; // Default idle direction

    public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
    {
        SpriteSheet.ActivateAnimation("IdleDown"); // Initial idle animation
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height, double time)
    {
        var pixelsToMove = time * _pixelsPerSecond;

        var x = Position.X + (int)(right * pixelsToMove);
        x -= (int)(left * pixelsToMove);

        var y = Position.Y - (int)(up * pixelsToMove);
        y += (int)(down * pixelsToMove);

        x = Math.Clamp(x, 10, width - 10);
        y = Math.Clamp(y, 24, height - 6);

        Position = (x, y);

        string newAnimation = null;
        if (up > 0) {
            newAnimation = "WalkUp";
        } else if (down > 0) {
            newAnimation = "WalkDown";
        } else if (left > 0) {
            newAnimation = "WalkLeft";
        } else if (right > 0) {
            newAnimation = "WalkRight";
        } else {
            newAnimation = _lastDirection.Replace("Walk", "Idle"); 
        }
        
        // Only activate the new animation if it's different from the current one
        if (newAnimation != _lastDirection || SpriteSheet.ActiveAnimation == null) {
            SpriteSheet.ActivateAnimation(newAnimation);
            _lastDirection = newAnimation;
        }
    }

}