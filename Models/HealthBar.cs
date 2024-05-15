using Silk.NET.Maths;

namespace TheAdventure.Models
{
    public class HealthBar
    {
        private int _maxHealth;
        private int _currentHealth;
        private int _barWidth;
        private int _barHeight;
        private byte _r;
        private byte _g;
        private byte _b;
        private byte _a;

        public HealthBar(int maxHealth, int barWidth, int barHeight, byte r, byte g, byte b, byte a)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _barWidth = barWidth;
            _barHeight = barHeight;
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }

        public void SetHealth(int currentHealth)
        {
            _currentHealth = currentHealth;
        }
        public int GetHealth()
        {
            return _currentHealth;
        }

        public void Render(GameRenderer renderer, Vector2D<int> position)
        {
            float healthPercentage = (float)_currentHealth / _maxHealth;
            int filledWidth = (int)(_barWidth * healthPercentage);

            renderer.SetDrawColor(_r, _g, _b, _a);
            renderer.RenderFilledRectangle(position.X, position.Y, _barWidth, _barHeight, 128, 128, 128, 255);
            renderer.RenderFilledRectangle(position.X, position.Y, filledWidth, _barHeight, _r, _g, _b, _a);
        }
    }

}
