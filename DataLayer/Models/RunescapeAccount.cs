using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class RunescapeAccount
    {
        public required string RunescapeName { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public ICollection<CollectionLog> CollectionLogs { get; set; }

    }
}
