using System;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace TheAdventure.Models
{
    public unsafe class HealthBar
    {
        private readonly Sdl _sdl;
        private readonly Renderer* _renderer;
        private readonly int _maxHealth;
        private int _currentHealth;
        public int _superPowerIconTextureId = -1;

        public HealthBar(Sdl sdl, Renderer* renderer, int maxHealth)
        {
            _sdl = sdl;
            _renderer = renderer;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void DecreaseHealth(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
            }
        }

        public void IncreaseHealth(int amount)
        {
            _currentHealth += amount;
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }
        }

        public void LoadSuperPowerIcon(GameRenderer renderer, string filePath)
        {
            _superPowerIconTextureId = renderer.LoadTexture(filePath, out _);
        }

        public void Render(int x, int y, int width, int height)
        {
            // Background
            _sdl.SetRenderDrawColor(_renderer, 255, 0, 0, 255);
            var backgroundRect = new Rectangle<int>(x, y, width, height);
            _sdl.RenderFillRect(_renderer, &backgroundRect);

            // Foreground
            _sdl.SetRenderDrawColor(_renderer, 0, 255, 0, 255);
            var healthRect = new Rectangle<int>(x, y, (int)(width * ((float)_currentHealth / _maxHealth)), height);
            _sdl.RenderFillRect(_renderer, &healthRect);
        }

        public void RenderSuperPower(SuperPower superPower, int x, int y, int width, int height, GameRenderer renderer)
        {
            // Render the icon
            if (_superPowerIconTextureId >= 0)
            {
                var srcRect = new Rectangle<int>(0, 0, 512, 512);
                var dstRect = new Rectangle<int>(x, y, width, height);
                renderer.RenderTexture(_superPowerIconTextureId, srcRect, dstRect);
            }

            // Render cooldown timer as a rectangle
            var cooldownTime = superPower.GetCooldownTimeRemaining();
            if (cooldownTime > 0)
            {
                var cooldownHeight = (int)(height * ((float)cooldownTime / SuperPower.CooldownTime));
                _sdl.SetRenderDrawColor(_renderer, 255, 255, 255, 128); // Semi-transparent white
                var cooldownRect = new Rectangle<int>(x, y + height - cooldownHeight, width, cooldownHeight);
                _sdl.RenderFillRect(_renderer, &cooldownRect);
            }
        }
    }
}
