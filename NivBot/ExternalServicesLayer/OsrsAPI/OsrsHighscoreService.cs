using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System.Net;
using System.Net.Http.Json;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    public sealed class OsrsHighscoreService(HttpClient httpClient) : IOsrsHighscoreService
    {
        // Returns null when the player is not on the hiscores (404).
        // Throws HttpRequestException when the hiscores are down or answer with another error.
        public async Task<PlayerStats?> GetPlayerStatsAsync(string name)
        {
            using var response = await httpClient.GetAsync($"index_lite.json?player={Uri.EscapeDataString(name)}");
            if (response.StatusCode == HttpStatusCode.NotFound) { return null; }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PlayerStats>();
        }
    }
}
