using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class ActivityTaskBlocklist
    {
        public int GoodplaceUserId { get; set; }
        public GoodplaceUser GoodplaceUser { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }


    }
    
}
