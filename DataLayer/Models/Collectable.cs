using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Collectable
    {
        public int Id { get; set; }
        public int? OsrsId { get; set; }
        public required string OsrsName { get; set; }
        public int Points { get; set; }
    }
}
