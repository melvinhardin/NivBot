namespace NivBot.Features.Shared
{
    public static class OsrsNames
    {
        // OSRS names are case insensitive and treat spaces, underscores and hyphens as the same character.
        // Every name stored in or compared against RunescapeAccounts goes through this.
        public static string Normalize(string name)
        {
            string cleaned = name
                .Replace(' ', ' ')
                .Replace('_', ' ')
                .Replace('-', ' ')
                .ToLowerInvariant();

            return string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
