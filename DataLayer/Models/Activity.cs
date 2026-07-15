using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public int OsrsId { get; set; }
        public string OsrsName { get; set; }
        public int Points { get; set; }
        
    }
}
