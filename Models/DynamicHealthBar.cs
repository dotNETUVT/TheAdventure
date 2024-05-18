using Silk.NET.Maths;

namespace TheAdventure.Models
{
    public class DynamicHealthBar
    {
        private int _maxHealth;
        private int _currentHealth;
        private int _barWidth;
        private int _barHeight;

        public DynamicHealthBar(int maxHealth, int barWidth, int barHeight)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _barWidth = barWidth;
            _barHeight = barHeight;
        }
        public void SetHealth(int currentHealth)
        {
            _currentHealth = currentHealth < 0 ? 0 : (currentHealth > _maxHealth ? _maxHealth : currentHealth);
        }
        public void Render(GameRenderer renderer, Vector2D<int> position)
        {
            float healthLevel = (float)_currentHealth / _maxHealth;
            int barAdjusted = (int)(_barWidth * healthLevel);
            byte r = 0, g = 255, b = 0;
            if (healthLevel <= 0.20f)
            {
                r = 255;
                g = 0;
                b = 0;
            }

            renderer.RenderFilledRectangle(position.X, position.Y, _barWidth, _barHeight, 128, 128, 128, 255);
            renderer.RenderFilledRectangle(position.X, position.Y, barAdjusted, _barHeight, r, g, b, 255);
        }
    }
}
