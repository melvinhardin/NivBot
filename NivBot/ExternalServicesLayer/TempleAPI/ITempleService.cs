using NivBot.ExternalServicesLayer.TempleAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NivBot.ExternalServicesLayer.TempleAPI
{
    public interface ITempleService
    {
        Task<List<ParsedMember>> GetGroupCollectionsAsync(int groupId);
        Task<Dictionary<int, string>?> GetItemListAsync();
        Task<SingleMember> GetAccountCollection(string osrsName);
    }
}
