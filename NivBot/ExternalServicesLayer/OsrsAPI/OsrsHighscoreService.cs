using Microsoft.AspNetCore.Http;
using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    public sealed class OsrsHighscoreService(HttpClient client) : IOsrsHighscoreService
    {
        public async Task<PlayerStats?> GetPlayerStatsAsync(string name)
        {
            try { 
                var response = await client.GetAsync($"index_lite.json?player={Uri.EscapeDataString(name)}");
                response.EnsureSuccessStatusCode();
                PlayerStats? content = await response.Content.ReadFromJsonAsync<PlayerStats>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
