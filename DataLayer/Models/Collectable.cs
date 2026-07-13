using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Collectable
    {
        public int Id { get; set; }
        public required string OsrsName { get; set; }
        public required int Points { get; set; }
    }
}
