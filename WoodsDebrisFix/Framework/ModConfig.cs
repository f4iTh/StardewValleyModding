namespace WoodsDebrisFix.Framework
{
    internal class ModConfig
    {
        // Chance of 'rare' woods debris triggering (1 / Rarechance)
        // Default: 100.
        public int Chance { get; set; } = 100;

        // The luck increase if 'rare' woods debris triggers.
        // Default: 0.035
        public double Luck { get; set; } = 0.035;

        // Determines if 'rare debris' addition should be enabled
        // Default: true
        public bool Enabled { get; set; } = true;
    }
}
