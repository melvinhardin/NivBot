namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    // Bound from the "Osrs" section of appsettings.json / environment variables.
    public sealed class OsrsOptions
    {
        // Any existing account works: the hiscores return every activity for every player.
        // Used by /syncactivities to read the list of activities.
        public string ReferencePlayer { get; set; } = "niv lem";
    }
}
