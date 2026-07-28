using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class CollectionLog
    {
        public int Amount { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int RunescapeAccountId { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }
        

    }
}
