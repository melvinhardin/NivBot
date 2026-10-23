using NivBot.ExternalServicesLayer.OsrsAPI.Models;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    public interface IOsrsHighscoreService
    {
        // Returns null when the player is not on the hiscores.
        // Throws HttpRequestException when the hiscores can't be reached.
        // Unranked skills and activities come back as -1.
        Task<PlayerStats?> GetPlayerStatsAsync(string name);
    }
}
