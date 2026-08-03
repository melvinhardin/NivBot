using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NivBot.ExternalServicesLayer.TempleAPI
{
    public interface ITempleService
    {
        Task<TempleGroup?> GetGroupCollectionsAsync(int groupId);
        Task<Dictionary<string, string>?> GetItemListAsync();
    }
}
