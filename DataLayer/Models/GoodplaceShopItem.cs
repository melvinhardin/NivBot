using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.DataLayer.Models
{
    public class GoodplaceShopItem
    {
        public int Id { get; set; }
        public required string Description { get; set; }
        public required int Price { get; set; }

    }
}
