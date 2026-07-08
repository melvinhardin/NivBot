using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Wallet
    {
        public int WalletID { get; set; }
        public int GoodplacePoints { get; set; }
        public int GoodplaceCurrency { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        
    }
}
