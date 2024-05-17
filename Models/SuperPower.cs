namespace TheAdventure.Models
{
    public class SuperPower
    {
        private DateTimeOffset _lastActivated;
        private const int CooldownSeconds = 30;
        private const int DurationSeconds = 5;

        public bool IsActive { get; private set; }
        public bool IsOnCooldown => (DateTimeOffset.Now - _lastActivated).TotalSeconds < CooldownSeconds;
        public static int CooldownTime => CooldownSeconds;  // Propiedad pública para acceder a CooldownSeconds

        public void Activate()
        {
            if (IsOnCooldown) return;

            IsActive = true;
            _lastActivated = DateTimeOffset.Now;
        }

        public void Update()
        {
            if (IsActive && (DateTimeOffset.Now - _lastActivated).TotalSeconds > DurationSeconds)
            {
                IsActive = false;
            }
        }

        public int GetCooldownTimeRemaining()
        {
            var timeSinceLastActivation = (DateTimeOffset.Now - _lastActivated).TotalSeconds;
            return Math.Max(0, CooldownSeconds - (int)timeSinceLastActivation);
        }
    }
}