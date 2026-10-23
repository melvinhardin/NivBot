namespace NivBot.ExternalServicesLayer
{
    // An error answered by Wise Old Man or TempleOSRS, with their message so it can be shown to an admin.
    public class ExternalApiException(string message) : Exception(message);

    // One line of a competition's standings, best first.
    public sealed record CompetitionStanding(string Name, long Gained);
}
