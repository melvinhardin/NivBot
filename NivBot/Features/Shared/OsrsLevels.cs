namespace NivBot.Features.Shared
{
    public static class OsrsLevels
    {
        public const int Level99Xp = 13_034_431;
        public const int MaxXp = 200_000_000;

        // XP needed for each level (index = level), using the in-game formula.
        private static readonly int[] XpForLevel = BuildTable();

        public static int FromXp(long xp)
        {
            for (int level = 99; level > 1; level--)
            {
                if (xp >= XpForLevel[level]) { return level; }
            }
            return 1;
        }

        private static int[] BuildTable()
        {
            var table = new int[100];
            double points = 0;
            for (int level = 1; level < 99; level++)
            {
                points += Math.Floor(level + 300 * Math.Pow(2, level / 7.0));
                table[level + 1] = (int)Math.Floor(points / 4);
            }
            return table;
        }
    }
}
