namespace NivBot.DataLayer.Models
{
    // Remembers that the "close to" post for a role was already made for a user, so it's posted only once.
    public class RoleProgressNotice
    {
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; } = null!;
        public long DiscordRoleId { get; set; }
        public DiscordRole DiscordRole { get; set; } = null!;
        public DateTime NotifiedAt { get; set; }
    }
}
