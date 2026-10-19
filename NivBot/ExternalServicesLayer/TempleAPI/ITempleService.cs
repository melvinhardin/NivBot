using NivBot.ExternalServicesLayer.TempleAPI.Models;

namespace NivBot.ExternalServicesLayer.TempleAPI
{
    public interface ITempleService
    {
        Task<List<ParsedMember>> GetGroupCollectionsAsync(int groupId);
        Task<Dictionary<int, string>?> GetItemListAsync();

        // Null when Temple has no collection log for the player (not tracked, or never synced with the TempleOSRS RuneLite plugin).
        Task<List<ParsedItem>?> GetPlayerCollectionAsync(string osrsName);

        // Creates a group competition. Returns Temple's competition id and the key needed to delete it.
        Task<(string Id, string Key)> CreateCompetitionAsync(string name, string metric, DateTime startUtc, DateTime endUtc, int groupId, string groupKey);
        Task<List<CompetitionStanding>> GetCompetitionStandingsAsync(string competitionId);
        Task DeleteCompetitionAsync(string competitionId, string key);
    }
}
