using Silk.NET.Maths;
using TheAdventure;

public class PlayerObject : GameObject
{
    /// <summary>
    /// Player X position in world coordinates.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Player Y position in world coordinates.
    /// </summary>
    public int Y { get; set; }

    // Offset player sprite to have world position at x=24px y=42px

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0,0,48,48);
    private AnimatedGameObject _currentAnimation;
    private readonly AnimatedGameObject _movingUp;
    private readonly AnimatedGameObject _movingDown;
    private readonly AnimatedGameObject _movingLeft;
    private readonly AnimatedGameObject _movingRight;
    private readonly AnimatedGameObject _attackingUp;
    private readonly AnimatedGameObject _attackingDown;
    private readonly AnimatedGameObject _attackingLeft;
    private readonly AnimatedGameObject _attackingRight;
    private bool _attack = false;
    //private int _textureId;
    private int _pixelsPerSecond = 128;

    public PlayerObject(int id) : base(id)
    {
        _movingRight = new AnimatedGameObject(Path.Combine("Assets", "player_right.png"), 2, 1, 6, 6, 1, X, Y, true);
        _movingUp = new AnimatedGameObject(Path.Combine("Assets", "player_up.png"), 2, 1, 6, 6, 1, X, Y, true);
        _movingDown = new AnimatedGameObject(Path.Combine("Assets", "player_down.png"), 2, 1, 6, 6, 1, X, Y, true);
        _movingLeft = new AnimatedGameObject(Path.Combine("Assets", "player_left.png"), 2, 1, 6, 6, 1, X, Y, true);

        _attackingRight = new AnimatedGameObject(Path.Combine("Assets", "attack_right.png"), 2, 1, 4, 4, 1, X, Y, true);
        _attackingUp = new AnimatedGameObject(Path.Combine("Assets", "attack_up.png"), 2, 1, 4, 4, 1, X, Y, true);
        _attackingDown = new AnimatedGameObject(Path.Combine("Assets", "attack_down.png"), 2, 1, 4, 4, 1, X, Y, true);
        _attackingLeft = new AnimatedGameObject(Path.Combine("Assets", "attack_left.png"), 2, 1, 4, 4, 1, X, Y, true);

        //_textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
        _currentAnimation = _movingDown; 
        UpdateScreenTarget();
    }

    private void UpdateScreenTarget(){
        var targetX = X + 24;
        var targetY = Y - 42;

        _target = new Rectangle<int>(targetX, targetY, 48, 48);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int time)
    {
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        X += (int)(right * pixelsToMove);
        X -= (int)(left * pixelsToMove);
        Y -= (int)(up * pixelsToMove);
        Y += (int)(down * pixelsToMove);

        if (up > 0)
        {
            if (_attack) _currentAnimation = _attackingUp;
            else _currentAnimation = _movingUp;
        }
        else if (right > 0)
        {
            if (_attack) _currentAnimation = _attackingRight;
            else _currentAnimation = _movingRight;
        }
        else if (down > 0)
        {
            if (_attack) _currentAnimation = _attackingDown;
            else _currentAnimation = _movingDown;
        }
        else if (left > 0)
        {
            if (_attack) _currentAnimation = _attackingLeft;
            else _currentAnimation = _movingLeft;
        }

        _currentAnimation.UpdateAnimationPosition(X, Y);
 

        _currentAnimation.Update(time);
        _source = _currentAnimation.TextureSource;

        _currentAnimation.ResumeAnimation();

        UpdateScreenTarget();
    }

    public void Attack(bool isAttacking)
    {
        _attack = isAttacking;
    }

    public void Render(GameRenderer renderer){
        //renderer.RenderTexture(_textureId, _source, _target);
        _currentAnimation.Render(renderer); 
    }
}