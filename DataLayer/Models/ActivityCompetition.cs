using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class ActivityCompetition
    {
        public int Id { get; set; }
        public CompetitionProviderDetails CompetitionProviderDetails { get; set; }
        public Activity Activity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
