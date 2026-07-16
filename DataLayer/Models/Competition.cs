using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class Competition
    {
        public required APIProviders CompetitionProvider { get; set; }
        public required string ExternalId { get; set; }
        public required string CompetitionKey { get; set; }
    }
}
