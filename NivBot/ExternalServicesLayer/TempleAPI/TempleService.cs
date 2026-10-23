using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NivBot.ExternalServicesLayer.TempleAPI
{
    // A class that requires an httpclient injection with the baseadress of the Templeosrs api.
    // API docs: https://templeosrs.com/api_doc.php
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

        public async Task<List<ParsedItem>?> GetPlayerCollectionAsync(string osrsName)
        {
            string requestUrl = $"collection-log/player_collection_log.php?player={Uri.EscapeDataString(osrsName)}&categories=all&onlyitems=1";
            using var response = await httpClient.GetAsync(requestUrl);
            if (response.StatusCode == HttpStatusCode.NotFound) { return null; }
            response.EnsureSuccessStatusCode();

            using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || root.TryGetProperty("error", out _)) { return null; }

            JsonElement data = root.TryGetProperty("data", out JsonElement wrapped) ? wrapped : root;
            if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("items", out JsonElement items)) { return null; }

            var result = new Dictionary<int, int>();
            CollectItems(items, result);
            return result.Select(x => new ParsedItem { OsrsId = x.Key, Amount = x.Value }).ToList();
        }

        public async Task<(string Id, string Key)> CreateCompetitionAsync(string name, string metric, DateTime startUtc, DateTime endUtc, int groupId, string groupKey)
        {
            var body = new Dictionary<string, object>
            {
                ["name"] = name,
                ["skill"] = metric,
                ["team-comp"] = 0,
                ["start-date"] = ToUnixSeconds(startUtc),
                ["end-date"] = ToUnixSeconds(endUtc),
                ["group-id"] = groupId,
                ["group-key"] = groupKey,
                ["group-sync"] = 1
            };
            using var response = await httpClient.PostAsJsonAsync("competition_create.php", body);
            using JsonDocument document = await ReadResponseAsync(response);

            JsonElement data = document.RootElement.TryGetProperty("data", out JsonElement wrapped) ? wrapped : document.RootElement;
            string? id = FindString(data, "competition_id", "id")
                ?? (data.TryGetProperty("info", out JsonElement info) ? FindString(info, "id") : null);
            string? key = FindString(data, "key", "competition_key");
            if (id == null || key == null)
            {
                throw new ExternalApiException("TempleOSRS created the competition but didn't return its id and key.");
            }
            return (id, key);
        }

        public async Task<List<CompetitionStanding>> GetCompetitionStandingsAsync(string competitionId)
        {
            using var response = await httpClient.GetAsync($"competition_info_v2.php?id={Uri.EscapeDataString(competitionId)}");
            using JsonDocument document = await ReadResponseAsync(response);

            JsonElement data = document.RootElement.TryGetProperty("data", out JsonElement wrapped) ? wrapped : document.RootElement;
            if (!data.TryGetProperty("participants", out JsonElement participants)) { return []; }

            // Participants come keyed by rank (as an object or an array)
            IEnumerable<JsonElement> rows = participants.ValueKind switch
            {
                JsonValueKind.Array => participants.EnumerateArray().ToList(),
                JsonValueKind.Object => participants.EnumerateObject().Select(x => x.Value).ToList(),
                _ => new List<JsonElement>()
            };

            var standings = new List<CompetitionStanding>();
            foreach (JsonElement row in rows)
            {
                if (row.ValueKind != JsonValueKind.Object) { continue; }
                string? name = FindString(row, "username", "player_name_with_capitalization");
                if (name == null) { continue; }
                standings.Add(new CompetitionStanding(name, FindLong(row, "gain") ?? 0));
            }
            return standings.OrderByDescending(x => x.Gained).ToList();
        }

        public async Task DeleteCompetitionAsync(string competitionId, string key)
        {
            var body = new Dictionary<string, object> { ["id"] = competitionId, ["key"] = key };
            using var response = await httpClient.PostAsJsonAsync("competition_delete.php", body);
            using JsonDocument result = await ReadResponseAsync(response);
        }

        private static long ToUnixSeconds(DateTime utc) =>
            new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeSeconds();

        // Temple answers errors as {"error": {...}} or {"error": "..."}, sometimes with a 200 status.
        private static async Task<JsonDocument> ReadResponseAsync(HttpResponseMessage response)
        {
            string text = await response.Content.ReadAsStringAsync();
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(text);
            }
            catch (JsonException)
            {
                throw new ExternalApiException($"TempleOSRS answered {(int)response.StatusCode} without JSON.");
            }

            if (document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty("error", out JsonElement error))
            {
                string message = error.ValueKind == JsonValueKind.Object
                    ? FindString(error, "message", "Message") ?? error.ToString()
                    : error.ToString();
                document.Dispose();
                throw new ExternalApiException($"TempleOSRS: {message}");
            }
            if (!response.IsSuccessStatusCode || document.RootElement.ValueKind != JsonValueKind.Object)
            {
                document.Dispose();
                throw new ExternalApiException($"TempleOSRS answered {(int)response.StatusCode}.");
            }
            return document;
        }

        // Collects item id -> count from the shapes Temple uses for item lists: arrays of {"id": .., "count": ..},
        // objects keyed by item id (value is the count or an object with a count), possibly grouped per category.
        private static void CollectItems(JsonElement element, Dictionary<int, int> result)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray()) { CollectItems(child, result); }
                return;
            }
            if (element.ValueKind != JsonValueKind.Object) { return; }

            long? id = FindLong(element, "id", "item_id");
            if (id != null)
            {
                int count = (int)(FindLong(element, "count", "amount", "quantity") ?? 1);
                result[(int)id.Value] = Math.Max(count, 1);
                return;
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (int.TryParse(property.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId))
                {
                    if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out int count))
                    {
                        result[itemId] = Math.Max(count, 1);
                        continue;
                    }
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        result[itemId] = (int)Math.Max(FindLong(property.Value, "count", "amount", "quantity") ?? 1, 1);
                        continue;
                    }
                }
                // Categories or other nesting
                CollectItems(property.Value, result);
            }
        }

        private static string? FindString(JsonElement element, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object) { return null; }
            foreach (string name in names)
            {
                if (!element.TryGetProperty(name, out JsonElement value)) { continue; }
                if (value.ValueKind == JsonValueKind.String) { return value.GetString(); }
                if (value.ValueKind == JsonValueKind.Number) { return value.GetRawText(); }
            }
            return null;
        }

        private static long? FindLong(JsonElement element, params string[] names)
        {
            if (element.ValueKind != JsonValueKind.Object) { return null; }
            foreach (string name in names)
            {
                if (!element.TryGetProperty(name, out JsonElement value)) { continue; }
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number)) { return number; }
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double floating)) { return (long)floating; }
                if (value.ValueKind == JsonValueKind.String
                    && long.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)) { return parsed; }
            }
            return null;
        }
    }
}
