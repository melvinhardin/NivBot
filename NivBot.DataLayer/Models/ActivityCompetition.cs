using NivBot.DataLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class ActivityCompetition
    {
        public int CompetitionProviderDetailsId { get; set; }
        public CompetitionProviderDetails CompetitionProviderDetails { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
