using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace NivBot.ExternalServicesLayer.TempleAPI
{
    // A class that requires an httpclient injection with the baseadress of the Templeosrs api.
    public sealed class TempleService(HttpClient httpClient) : ITempleService
    {
        // Get the collectionlogs of all members that belong to a group. Uses the https://templeosrs.com/api_doc.php#Group_Collection_Log endpoint. Returns a list of ParsedMembers
        public async Task<List<ParsedMember>> GetGroupCollectionsAsync(int groupId) { 
        
            string requestUrl = $"collection-log/group_collection_log.php?group={Uri.EscapeDataString(groupId.ToString())}&categories=all&includecount=1";
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, TempleGroup>>();
            List<ParsedMember> parsedMemberData = new();
            // Parse all the items into a list of ParsedMembers containing all the collectionlog data per member
            foreach (var x in content.Values.First().Members)
            { 
                List<ParsedItem> parsedItemData = new();
                foreach (var y in x.Items)
                {
                    parsedItemData.Add(new ParsedItem { OsrsId = Int32.Parse(y.Key), Amount = y.Value });

                }
                parsedMemberData.Add(new ParsedMember { OsrsName = x.Player, Items = parsedItemData });
            }
            return parsedMemberData;
            
        }

        public async Task<SingleMember> GetAccountCollection(string osrsName)
        {
            string requestUrl = $"collection-log/player_collection_log.php?player={Uri.EscapeDataString(osrsName)}&categories=all&onlyitems=1";
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<SingleMember>();
            return content;
        }


        // Gets a list of all items in the collectionlog currently on Templeosrs from the https://templeosrs.com/api_doc.php#Clog_List_Items endpoint. Returns a <int, string> Dictionary
        public async Task<Dictionary<int, string>?> GetItemListAsync()
        {
            string requestUrl = "collection-log/items.php";
            var response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, Dictionary<string, string>>>();
            Dictionary<int, string> formattedDict = new();

            foreach(var x in content["items"]) {
                formattedDict.Add(Int32.Parse(x.Key),x.Value);
            }
            return formattedDict;
        }
    }
}
