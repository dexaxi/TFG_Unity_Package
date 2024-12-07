namespace DUJAL.Systems.Dungeons.Utils
{
    using System;
    public static class RandomUtils
    {
        public static long Seed { get; private set; }
        private static long OriginalSeed = -1;

        public static void RegenerateSeed()
        {
            OriginalSeed = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            SilentUpdateSeed();
        }

        private static void SilentUpdateSeed() 
        {
            if (OriginalSeed == -1) RegenerateSeed();
            Seed = (Seed * 3627219L + 24L) & long.MaxValue;
        }

        public static double GetRandomDouble(double max)
        {
            return GetRandomDouble() * max;
        }

        public static double GetRandomDouble()
        {
            SilentUpdateSeed();
            return Math.Abs(Seed) / long.MaxValue;
        }
    }
}