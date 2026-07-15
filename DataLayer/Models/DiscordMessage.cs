using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class DiscordMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}
