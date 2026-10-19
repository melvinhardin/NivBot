using NivBot.DataLayer.Enums;

namespace NivBot.DataLayer.Models
{
    // A competition created on Wise Old Man or TempleOSRS. The metric and times are in SkillCompetition or ActivityCompetition.
    public class CompetitionProviderDetails
    {
        public int Id { get; set; }
        public required APIProviders CompetitionProvider { get; set; }

        // The competition id at the provider.
        public required string ExternalId { get; set; }

        // Verification code (WOM) / key (Temple) needed to change or delete the competition.
        public required string CompetitionKey { get; set; }

        public required string Title { get; set; }

        // Where /competition create was used; the results are posted in that channel.
        public long GuildId { get; set; }
        public long ChannelId { get; set; }

        // Set once the results were posted and the points awarded.
        public DateTime? FinishedAt { get; set; }
    }
}
