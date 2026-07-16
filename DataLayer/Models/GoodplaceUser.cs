using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class GoodplaceUser
    {
        public int Id { get; set; }
        public required long DiscordUserId { get; set; }

        public Wallet Wallet { get; set; }
        public ICollection<RunescapeAccount> RunescapeAccounts { get; set; }


        public GoodplaceActivityTask GoodplaceActivityTask { get; set; }
        public GoodplaceSkillTask GoodplaceSkillTasks { get; set; }

    }
}
