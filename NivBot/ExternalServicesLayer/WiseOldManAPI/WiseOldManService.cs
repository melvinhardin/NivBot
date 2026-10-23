using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace NivBot.ExternalServicesLayer.WiseOldManAPI
{
    public interface IWiseOldManService
    {
        // Creates a group competition. Returns WOM's competition id and the verification code needed to change it.
        Task<(string Id, string VerificationCode)> CreateCompetitionAsync(string title, string metric, DateTime startUtc, DateTime endUtc, int groupId, string groupVerificationCode);
        Task<List<CompetitionStanding>> GetCompetitionStandingsAsync(string competitionId);
        // Asks WOM to update every outdated participant. Only works before the competition ends.
        Task UpdateAllParticipantsAsync(string competitionId, string verificationCode);
        Task DeleteCompetitionAsync(string competitionId, string verificationCode);
    }

    // Requires an httpclient with the base address https://api.wiseoldman.net/v2/
    // API docs: https://docs.wiseoldman.net
    public sealed class WiseOldManService(HttpClient httpClient) : IWiseOldManService
    {
        public async Task<(string Id, string VerificationCode)> CreateCompetitionAsync(string title, string metric, DateTime startUtc, DateTime endUtc, int groupId, string groupVerificationCode)
        {
            var body = new Dictionary<string, object>
            {
                ["title"] = title,
                ["metric"] = metric,
                ["startsAt"] = ToIso(startUtc),
                ["endsAt"] = ToIso(endUtc),
                ["groupId"] = groupId,
                ["groupVerificationCode"] = groupVerificationCode
            };
            using var response = await httpClient.PostAsJsonAsync("competitions", body);
            using JsonDocument document = await ReadResponseAsync(response);

            JsonElement root = document.RootElement;
            string? id = root.TryGetProperty("competition", out JsonElement competition) ? GetString(competition, "id") : null;
            if (id == null) { throw new ExternalApiException("Wise Old Man created the competition but didn't return its id."); }

            // Group competitions are verified with the group's code
            string verificationCode = GetString(root, "verificationCode") ?? groupVerificationCode;
            return (id, verificationCode);
        }

        public async Task<List<CompetitionStanding>> GetCompetitionStandingsAsync(string competitionId)
        {
            using var response = await httpClient.GetAsync($"competitions/{Uri.EscapeDataString(competitionId)}");
            using JsonDocument document = await ReadResponseAsync(response);

            if (!document.RootElement.TryGetProperty("participations", out JsonElement participations)
                || participations.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var standings = new List<CompetitionStanding>();
            foreach (JsonElement participation in participations.EnumerateArray())
            {
                if (!participation.TryGetProperty("player", out JsonElement player)) { continue; }
                string? name = GetString(player, "displayName") ?? GetString(player, "username");
                if (name == null) { continue; }

                long gained = 0;
                if (participation.TryGetProperty("progress", out JsonElement progress))
                {
                    gained = GetLong(progress, "gained") ?? 0;
                }
                standings.Add(new CompetitionStanding(name, gained));
            }
            return standings.OrderByDescending(x => x.Gained).ToList();
        }

        public async Task UpdateAllParticipantsAsync(string competitionId, string verificationCode)
        {
            var body = new Dictionary<string, object> { ["verificationCode"] = verificationCode };
            using var response = await httpClient.PostAsJsonAsync($"competitions/{Uri.EscapeDataString(competitionId)}/update-all", body);
            using JsonDocument result = await ReadResponseAsync(response);
        }

        public async Task DeleteCompetitionAsync(string competitionId, string verificationCode)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"competitions/{Uri.EscapeDataString(competitionId)}")
            {
                Content = JsonContent.Create(new Dictionary<string, object> { ["verificationCode"] = verificationCode })
            };
            using var response = await httpClient.SendAsync(request);
            using JsonDocument result = await ReadResponseAsync(response);
        }

        private static string ToIso(DateTime utc) =>
            DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

        // WOM answers errors with a non-success status and {"message": "..."}.
        private static async Task<JsonDocument> ReadResponseAsync(HttpResponseMessage response)
        {
            string text = await response.Content.ReadAsStringAsync();
            JsonDocument? document = null;
            try
            {
                document = JsonDocument.Parse(text);
            }
            catch (JsonException)
            {
                // handled below
            }

            if (!response.IsSuccessStatusCode || document == null || document.RootElement.ValueKind != JsonValueKind.Object)
            {
                string? message = document != null && document.RootElement.ValueKind == JsonValueKind.Object
                    ? GetString(document.RootElement, "message") ?? GetString(document.RootElement, "code")
                    : null;
                document?.Dispose();
                throw new ExternalApiException($"Wise Old Man answered {(int)response.StatusCode}: {message ?? response.ReasonPhrase}");
            }
            return document;
        }

        private static string? GetString(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value)) { return null; }
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }

        private static long? GetLong(JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out JsonElement value)) { return null; }
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long number)) { return number; }
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double floating)) { return (long)floating; }
            return null;
        }
    }
}
