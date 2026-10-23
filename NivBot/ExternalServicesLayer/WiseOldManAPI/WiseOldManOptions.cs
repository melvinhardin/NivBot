namespace NivBot.ExternalServicesLayer.WiseOldManAPI
{
    // Bound from the "WiseOldMan" section of appsettings.json / environment variables.
    public sealed class WiseOldManOptions
    {
        // Your Wise Old Man group and its verification code, needed to create competitions. 0 = not set.
        public int GroupId { get; set; }
        public string GroupVerificationCode { get; set; } = "";

        // Optional, raises WOM's rate limit. Leave empty without one: WOM rejects unknown keys.
        public string ApiKey { get; set; } = "";
    }
}
