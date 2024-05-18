using System.Drawing;

namespace TheAdventure
{
    public class DayNightCycle
    {
        private const double DayLength = 60.0; // Length of a day in seconds
        private double _currentTime = 0.0; // Current time in seconds
        public Color BrightestDayColor { get; set; } = Color.FromArgb(255, 227, 93, 20);
        public Color DarkestNightColor { get; set; } = Color.FromArgb(255, 4, 30, 71);

        public void Update(double deltaTime)
        {
            // Update the current time
            _currentTime += deltaTime;

            // Loop the current time back to 0 when a full day has passed
            if (_currentTime > DayLength)
            {
                _currentTime -= DayLength;
            }
        }

        public Color GetCurrentColor()
        {
            // Calculate the current time as a percentage of the day length
            double timePercentage = _currentTime / DayLength;

            // Use a sinusoidal function to calculate the interpolation factor
            // This will result in a smooth transition between day and night
            double interpolationFactor = (Math.Cos(timePercentage * 2 * Math.PI) + 1) / 2;

            // Interpolate between the brightest day color and the darkest night color based on the interpolation factor
            int r = (int)(BrightestDayColor.R * interpolationFactor + DarkestNightColor.R * (1 - interpolationFactor));
            int g = (int)(BrightestDayColor.G * interpolationFactor + DarkestNightColor.G * (1 - interpolationFactor));
            int b = (int)(BrightestDayColor.B * interpolationFactor + DarkestNightColor.B * (1 - interpolationFactor));

            return Color.FromArgb(r, g, b);
        }
        public float GetLightLevel()
        {
            // Implement the logic to calculate the light level based on the current time of day
            // This is a simple implementation that returns 1.0 during the day and 0.0 during the night
            // Adjust this implementation as needed to get the desired day-night cycle effect
            return _currentTime < DayLength / 2 ? 1.0f : 0.0f;
        }
        
        public byte GetTransparencyLevel()
        {
            // Calculate the current time as a percentage of the day length
            double timePercentage = _currentTime / DayLength;

            // Use a sinusoidal function to calculate the interpolation factor
            // This will result in a smooth transition between day and night
            double interpolationFactor = (Math.Cos(timePercentage * 2 * Math.PI) + 1) / 2;

            // Interpolate between 128 (night) and 64 (day) based on the interpolation factor
            return (byte)(128 * (1 - interpolationFactor) + 64 * interpolationFactor);
        }
    }
}