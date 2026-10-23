using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class RunescapeAccount
    {
        public int Id { get; set; }
        public required string RunescapeName { get; set; }
        public bool syncedColLog { get; set; } = false;

        // "Overall" from the hiscores: total level and total XP. 0 until the account is ranked overall.
        public int OverallLevel { get; set; }
        public long OverallXp { get; set; }
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public ICollection<CollectionLog> CollectionLogs { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; }
        public ICollection<RunescapeStat> RunescapeStats { get; set; }

    }
}
