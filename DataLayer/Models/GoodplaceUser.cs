using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class GoodplaceUser
    {
        public int DiscordUUID { get; set; }
        public string DiscordUsername { get; set; }
        
        public ICollection<RunescapeAccount> RunescapeAccounts { get; set; }

    }
}
