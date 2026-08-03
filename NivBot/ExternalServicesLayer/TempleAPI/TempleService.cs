using Microsoft.AspNetCore.WebUtilities;
using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace NivBot.ExternalServicesLayer.TempleAPI
{
    public sealed class TempleService(HttpClient httpClient) : ITempleService
    {
        public async Task<TempleGroup?> GetGroupCollectionsAsync(int groupId) { 
        
            
            try
            {
                string requestUrl = $"collection-log/group_collection_log.php?group={Uri.EscapeDataString(groupId.ToString())}&categories=all&includecount=1";
                var response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, TempleGroup>>();
                return content.Values.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        public async Task<Dictionary<string, string>?> GetItemListAsync()
        {
            try
            {
                string requestUrl = "collection-log/items.php";
                var response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return content;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
