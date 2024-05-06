using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models.Data;

namespace TheAdventure.Models;

public class SpriteSheet
{
    public class Animation
    {
        public FramePosition StartFrame { get; set; }
        public FramePosition EndFrame { get; set; }
        public RendererFlip Flip { get; set; } = RendererFlip.None;
        public int DurationMs { get; set; }
        public bool Loop { get; set; }
    }

    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public FrameOffset FrameCenter { get; set; }
    public string? FileName { get; set; }
    public Animation? ActiveAnimation { get; private set; }
    public Dictionary<string, Animation> Animations { get; init; } = new();

    private int _textureId = -1;
    private float _opacity = 1.0f;  // Default opacity is 100%
    private Vector2D<float> _scale = new(1f, 1f); // Default scale
    private DateTimeOffset _animationStart = DateTimeOffset.MinValue;

    public static SpriteSheet? LoadSpriteSheet(string fileName, string folder, GameRenderer renderer)
    {
        try
        {
            var path = Path.Combine(folder, fileName);
            var json = File.ReadAllText(path);
            var spriteSheet = JsonSerializer.Deserialize<SpriteSheet>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            spriteSheet?.LoadTexture(renderer, folder);
            return spriteSheet;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sprite sheet from {fileName}: {ex.Message}");
            return null;
        }
    }

    public void LoadTexture(GameRenderer renderer, string? parentFolder = null)
    {
        var filePath = FileName;
        try
        {
            if (!string.IsNullOrWhiteSpace(parentFolder) && !string.IsNullOrWhiteSpace(FileName))
            {
                filePath = Path.Combine(parentFolder, FileName);
            }
            if (_textureId == -1 && !string.IsNullOrWhiteSpace(filePath))
            {
                _textureId = renderer.LoadTexture(filePath, out _);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading texture from {filePath}: {ex.Message}");
        }
    }

    public void ActivateAnimation(string name)
    {
        if (Animations.TryGetValue(name, out var animation))
        {
            ActiveAnimation = animation;
            _animationStart = DateTimeOffset.Now;
        }
    }

    public void Render(GameRenderer renderer, (int X, int Y) dest, double angle = 0.0, Point rotationCenter = new())
    {
        if (ActiveAnimation == null)
        {
            RenderTexture(renderer, dest, angle, rotationCenter);
        }
        else
        {
            RenderAnimation(renderer, dest, angle, rotationCenter);
        }
    }

    private void RenderTexture(GameRenderer renderer, (int X, int Y) dest, double angle, Point rotationCenter)
    {
        renderer.RenderTexture(_textureId,
            new Rectangle<int>(0, 0, FrameWidth, FrameHeight),
            new Rectangle<int>(dest.X - FrameCenter.OffsetX, dest.Y - FrameCenter.OffsetY, (int)(FrameWidth * _scale.X), (int)(FrameHeight * _scale.Y)),
            RendererFlip.None, angle, rotationCenter, _opacity);
    }

    private void RenderAnimation(GameRenderer renderer, (int X, int Y) dest, double angle, Point rotationCenter)
    {
        int totalFrames = (ActiveAnimation.EndFrame.Row - ActiveAnimation.StartFrame.Row) * ColumnCount +
                          ActiveAnimation.EndFrame.Col - ActiveAnimation.StartFrame.Col + 1;
        int currentFrame = (int)((DateTimeOffset.Now - _animationStart).TotalMilliseconds /
                                 (ActiveAnimation.DurationMs / (double)totalFrames)) % totalFrames;

        int currentRow = ActiveAnimation.StartFrame.Row + currentFrame / ColumnCount;
        int currentCol = ActiveAnimation.StartFrame.Col + currentFrame % ColumnCount;

        renderer.RenderTexture(_textureId,
            new Rectangle<int>(currentCol * FrameWidth, currentRow * FrameHeight, FrameWidth, FrameHeight),
            new Rectangle<int>(dest.X - FrameCenter.OffsetX, dest.Y - FrameCenter.OffsetY, (int)(FrameWidth * _scale.X), (int)(FrameHeight * _scale.Y)),
            ActiveAnimation.Flip, angle, rotationCenter, _opacity);
    }

    public void SetOpacity(float opacity) => _opacity = opacity;
    public void SetScale(float scaleX, float scaleY) => _scale = new Vector2D<float>(scaleX, scaleY);
}
