using Silk.NET.SDL;
using Silk.NET.Maths;

namespace TheAdventure.Models
{
    public unsafe class HealthBar
    {
        private readonly Sdl _sdl;
        private readonly Renderer* _renderer;
        private readonly int _maxHealth;
        private int _currentHealth;

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

        public void Render(int x, int y, int width, int height)
        {
            // Draw the background (red)
            _sdl.SetRenderDrawColor(_renderer, 255, 0, 0, 255);
            var backgroundRect = new Rectangle<int>(x, y, width, height);
            _sdl.RenderFillRect(_renderer, &backgroundRect);

            // Draw the foreground (green)
            int healthWidth = (int)((_currentHealth / (float)_maxHealth) * width);
            _sdl.SetRenderDrawColor(_renderer, 0, 255, 0, 255);
            var foregroundRect = new Rectangle<int>(x, y, healthWidth, height);
            _sdl.RenderFillRect(_renderer, &foregroundRect);
        }
    }
}
