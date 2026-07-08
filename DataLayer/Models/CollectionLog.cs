using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class CollectionLog
    {
        public int Ammount { get; set; }

        public int CollectableId { get; set; }
        public Collectable Collectable { get; set; }

        public string RunescapeAccountName { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }
        

    }
}
