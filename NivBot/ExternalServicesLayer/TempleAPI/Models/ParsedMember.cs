using System;
using System.Collections.Generic;
using System.Text;


namespace NivBot.ExternalServicesLayer.TempleAPI.Models
{
    public class ParsedMember
    {
        public string OsrsName { get; set; }
        public ICollection<ParsedItem> Items { get; set; }
    }
}
