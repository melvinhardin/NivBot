using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public required int OsrsId { get; set; }
        public required string OsrsName { get; set; }
        public int Points { get; set; } = 0;
        
    }
}
