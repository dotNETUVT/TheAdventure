using Silk.NET.Maths;

namespace TheAdventure.Models
{
    public class StaminaBar
    {
        private int _maximumStamina;
        private int _currentStamina;
        private int _barWidth;
        private int _barHeight;

        public StaminaBar(int maximumStamina, int barWidth, int barHeight)
        {
            _maximumStamina = maximumStamina;
            _currentStamina = maximumStamina;
            _barWidth = barWidth;
            _barHeight = barHeight;
        }
        public void SetStamina(int currentStamina)
        {
            _currentStamina = currentStamina < 0 ? 0 : (currentStamina > _maximumStamina ? _maximumStamina : currentStamina);
        }
        public void Render(GameRenderer renderer, Vector2D<int> position)
        {
            float staminaLevel = (float)_currentStamina / _maximumStamina;
            int barAdjusted = (int)(_barWidth * staminaLevel);
            byte r = 0, g = 255, b = 255;

            renderer.RenderBar(position.X, position.Y, _barWidth, _barHeight, 128, 128, 128, 255);
            renderer.RenderBar(position.X, position.Y, barAdjusted, _barHeight, r, g, b, 255);
        }
    }
}