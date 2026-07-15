using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    internal class OsrsHighscoreService : IOsrsHighscoreService
    {
        private readonly HttpClient _httpClient;
        

        public OsrsHighscoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://secure.runescape.com/m=hiscore_oldschool/index_lite.json");
        }
        public async Task<PlayerStats> GetPlayerStatsAsync(string name)
        {
            return await _httpClient.GetFromJsonAsync<PlayerStats>($"?player={name}");
        }
    }
}
