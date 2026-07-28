using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class RunescapeAccount
    {
        public int Id { get; set; }
        public required string RunescapeName { get; set; }
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public ICollection<CollectionLog> CollectionLogs { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; }
        public ICollection<RunescapeStat> RunescapeStats { get; set; }

    }
}
