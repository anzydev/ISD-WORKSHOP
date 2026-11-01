namespace CoinCatcher.Core
{
    public struct DiffTuning
    {
        public float botSpeed;
        public float botReaction;
        public float botNoise;
        public float botIdleChance;
        public float cannonInterval;
        public float goldChance;
        public int burstMin;
        public int burstMax;
    }

    public static class DifficultySettings
    {
        public static DiffTuning Get(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy:
                    return new DiffTuning
                    {
                        botSpeed = 2.2f, botReaction = 0.55f, botNoise = 3.5f, botIdleChance = 0.30f,
                        cannonInterval = 1.50f, goldChance = 0.08f, burstMin = 1, burstMax = 2
                    };
                case Difficulty.Hard:
                    return new DiffTuning
                    {
                        botSpeed = 4.0f, botReaction = 0.18f, botNoise = 0.6f, botIdleChance = 0.02f,
                        cannonInterval = 0.85f, goldChance = 0.16f, burstMin = 2, burstMax = 4
                    };
                default:
                    return new DiffTuning
                    {
                        botSpeed = 3.1f, botReaction = 0.35f, botNoise = 1.6f, botIdleChance = 0.12f,
                        cannonInterval = 1.15f, goldChance = 0.12f, burstMin = 1, burstMax = 3
                    };
            }
        }

        public static string Label(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy: return "EASY";
                case Difficulty.Hard: return "HARD";
                default: return "MEDIUM";
            }
        }
    }
}
