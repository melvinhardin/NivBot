using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class CollectionLog
    {
        public int Amount { get; set; }

        public int CollectableId { get; set; }
        public Collectable Collectable { get; set; }

        public string RunescapeId { get; set; }
        public RunescapeAccount RunescapeAccount { get; set; }
        

    }
}
