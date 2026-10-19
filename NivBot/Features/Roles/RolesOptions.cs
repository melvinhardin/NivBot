namespace NivBot.Features.Roles
{
    // Bound from the "Roles" section of appsettings.json / environment variables.
    public sealed class RolesOptions
    {
        // How often every member's roles are checked against the hiscores. 0 = only when an admin runs /roles sync.
        public TimeSpan SyncInterval { get; set; } = TimeSpan.FromHours(6);

        // Pause between members during a sync, to go easy on the hiscores.
        public TimeSpan DelayBetweenMembers { get; set; } = TimeSpan.FromSeconds(1);

        // Channel for the "close to a role" posts (close_trigger). 0 = no posts.
        public ulong AnnouncementChannelId { get; set; }
    }
}
