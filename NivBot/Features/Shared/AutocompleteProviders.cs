using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NivBot.DataLayer;

namespace NivBot.Features.Shared
{
    // Autocomplete for slash command parameters. Discord shows at most 25 suggestions.
    internal static class Autocomplete
    {
        public const int MaxChoices = 25;

        public static string Search(ApplicationCommandInteractionDataOption option) => (option.Value ?? "").Trim().ToLowerInvariant();

        // Discord allows 100 characters for a choice name and value
        public static string Clip(string text) => text.Length <= 100 ? text : text[..100];
    }

    // Bosses (activities with a row in the Bosses table). The value is the activity name.
    public class BossAutocompleteProvider(GoodplaceContext db) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            string search = Autocomplete.Search(option);
            List<string> bossNames = (await db.Bosses.AsNoTracking().Select(x => x.OsrsName).ToListAsync())
                .Select(x => x.ToLowerInvariant())
                .ToList();

            List<string> names = (await db.Activities
                    .AsNoTracking()
                    .Where(x => x.OsrsName.ToLower().Contains(search))
                    .OrderBy(x => x.OsrsName)
                    .Select(x => x.OsrsName)
                    .ToListAsync())
                .Where(x => bossNames.Contains(x.ToLowerInvariant()))
                .Take(Autocomplete.MaxChoices)
                .ToList();

            return names.Select(x => new ApplicationCommandOptionChoiceProperties(Autocomplete.Clip(x), Autocomplete.Clip(x)));
        }
    }

    // Any activity from the hiscores (bosses, clues, minigames). The value is the activity name.
    public class ActivityAutocompleteProvider(GoodplaceContext db) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            string search = Autocomplete.Search(option);
            List<string> names = await db.Activities
                .AsNoTracking()
                .Where(x => x.OsrsName.ToLower().Contains(search))
                .OrderBy(x => x.OsrsName)
                .Select(x => x.OsrsName)
                .Take(Autocomplete.MaxChoices)
                .ToListAsync();

            return names.Select(x => new ApplicationCommandOptionChoiceProperties(Autocomplete.Clip(x), Autocomplete.Clip(x)));
        }
    }

    // Collection log items. The value is the item's OSRS id, because item names aren't unique.
    public class ItemAutocompleteProvider(GoodplaceContext db) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            string search = Autocomplete.Search(option);
            var items = await db.Items
                .AsNoTracking()
                .Where(x => x.OsrsName.ToLower().Contains(search))
                .OrderBy(x => x.OsrsName)
                .Select(x => new { x.OsrsId, x.OsrsName, x.Points })
                .Take(Autocomplete.MaxChoices)
                .ToListAsync();

            return items.Select(x => new ApplicationCommandOptionChoiceProperties(
                Autocomplete.Clip($"{x.OsrsName} ({x.Points} points, id {x.OsrsId})"),
                x.OsrsId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        }
    }
}
