using Silk.NET.Maths;
using TheAdventure;
using System.IO;

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
    private MovementDirection _lastMovementDirection = MovementDirection.None;

    // Movement directions enum
    private enum MovementDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    private Rectangle<int> _source = new Rectangle<int>(0, 0, 48, 48);
    private Rectangle<int> _target = new Rectangle<int>(0, 0, 48, 48);
    private int _textureId;
    private int _flippedTextureId;
    private int _NewtextureId;
    private int _pixelsPerSecond = 128;
    private int _spriteWidth = 48;
    private int _spriteHeight = 48;
    private bool _isMoving = false;
    private int _spritesPerRow = 6;
    private int _currentFrame = 0;
    private int _totalFrames = 6;
    public PlayerObject(int id) : base(id)
    {
        // Load textures for both normal and flipped player
        _textureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player.png"), out var textureData);
        _flippedTextureId = GameRenderer.LoadTexture(Path.Combine("Assets", "player_flipped.png"), out var flippedTextureData);

        UpdateScreenTarget();
    }

    private void UpdateScreenTarget()
    {
        var targetX = X + 24;
        var targetY = Y - 42;

        _target = new Rectangle<int>(targetX, targetY, 88, 88);
    }

    public void UpdatePlayerPosition(double up, double down, double left, double right, int time)
    {
        var pixelsToMove = (time / 1000.0) * _pixelsPerSecond;

        // Determine if there is movement
        bool isMoving = up != 0 || down != 0 || left != 0 || right != 0;

        // Update the flag to track movement status
        _isMoving = isMoving;

        // If there is movement, update position and direction
        if (isMoving)
        {
            X += (int)(right * pixelsToMove);
            X -= (int)(left * pixelsToMove);
            Y -= (int)(up * pixelsToMove);
            Y += (int)(down * pixelsToMove);

            // Update animation frame based on movement direction
            if (right != 0)
            {
                UpdateSpriteForRightMovement();
                _lastMovementDirection = MovementDirection.Right;

            }
            else if (left != 0)
            {
                UpdateSpriteForLeftMovement();
                _lastMovementDirection = MovementDirection.Left;
            }
            else if (up != 0)
            {
                UpdateSpriteForUpMovement();
                _lastMovementDirection = MovementDirection.Up;
            }
            else if (down != 0)
            {
                UpdateSpriteForDownMovement();
                _lastMovementDirection = MovementDirection.Down;
            }
        }
        else if (_isMoving)
        {
            // If no movement but was moving before, set the sprite to the last movement direction
            switch (_lastMovementDirection)
            {
                case MovementDirection.Up:
                    UpdateSpriteForUpMovement();
                    break;
                case MovementDirection.Down:
                    UpdateSpriteForDownMovement();
                    break;
                case MovementDirection.Left:
                    UpdateSpriteForLeftMovement();
                    break;
                case MovementDirection.Right:
                    UpdateSpriteForRightMovement();
                    break;
                case MovementDirection.None:
                default:
                    // If no movement recorded, set to default sprite or standing still sprite
                    // You can adjust this part according to your sprite sheet and desired behavior
                    _source = new Rectangle<int>(0, 0, _spriteWidth, _spriteHeight);
                    break;
            }
        }

        // Update the current frame for animation only if movement keys are pressed
        if (_isMoving)
            _currentFrame = (_currentFrame + 1) % _totalFrames;

        UpdateScreenTarget();
    }

    void UpdateSpriteForRightMovement()
    {
        // Calculate the source rectangle for the right movement
        int column = _currentFrame % _spritesPerRow;
        int row = 4; // Assuming the second row is for right movement frames in your sprite sheet
        int sourceX = column * _spriteWidth;
        int sourceY = row * _spriteHeight;
        _source = new Rectangle<int>(sourceX, sourceY, _spriteWidth, _spriteHeight);

        _NewtextureId = _textureId;

    }

    void UpdateSpriteForLeftMovement()
    {
        int column = _currentFrame % _spritesPerRow;
        int row = 4;
        int sourceX = column * _spriteWidth; // Start from the right side of the current frame
        int sourceY = row * _spriteHeight;
        _source = new Rectangle<int>(sourceX, sourceY, _spriteWidth, _spriteHeight);

        _NewtextureId = _flippedTextureId;

    }

    void UpdateSpriteForUpMovement()
    {
        int column = _currentFrame % _spritesPerRow;
        int row = 5;
        int sourceX = column * _spriteWidth;
        int sourceY = row * _spriteHeight;
        _source = new Rectangle<int>(sourceX, sourceY, _spriteWidth, _spriteHeight);

        _NewtextureId = _textureId;

    }

    void UpdateSpriteForDownMovement()
    {
        int column = _currentFrame % _spritesPerRow;
        int row = 3;
        int sourceX = column * _spriteWidth;
        int sourceY = row * _spriteHeight;
        _source = new Rectangle<int>(sourceX, sourceY, _spriteWidth, _spriteHeight);

        _NewtextureId = _textureId;

    }

    public void Render(GameRenderer renderer)
    {
        renderer.RenderTexture(_NewtextureId, _source, _target);
    }
}
