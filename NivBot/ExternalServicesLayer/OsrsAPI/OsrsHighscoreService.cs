using Microsoft.AspNetCore.Http;
using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    public sealed class OsrsHighscoreService(HttpClient httpClient) : IOsrsHighscoreService
    {
        // Try to get the highscore from osrs, if a 404 is thrown return null
        public async Task<PlayerStats?> GetPlayerStatsAsync(string name)
        {
            
                var response = await httpClient.GetAsync($"index_lite.json?player={Uri.EscapeDataString(name)}");
                response.EnsureSuccessStatusCode();
                PlayerStats? content = await response.Content.ReadFromJsonAsync<PlayerStats>();
                return content;
            
        }
    }
}
