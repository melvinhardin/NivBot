using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI.Models
{
    public class Activities
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required int Rank { get; set; }
        public required int Score { get; set; }
    }
}
