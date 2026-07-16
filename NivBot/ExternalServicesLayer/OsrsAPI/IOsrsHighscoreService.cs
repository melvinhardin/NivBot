using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace NivBot.ExternalServicesLayer.OsrsAPI
{
    public interface IOsrsHighscoreService
    {
        Task<PlayerStats?> GetPlayerStatsAsync(string name);
    }
}
