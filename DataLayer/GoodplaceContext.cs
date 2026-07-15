using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Npgsql;
using NivBot.DataLayer.Models;

namespace NivBot.DataLayer
{
    public abstract class GoodplaceContext : DbContext
    {
        public bool LoggingEnabled { get; set; }

        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<RunescapeAccount> RunescapeAccounts => Set<RunescapeAccount>();
        public DbSet<GoodplaceUser> GoodplaceUsers => Set<GoodplaceUser>();
        public DbSet<CollectionLog> CollectionLogs => Set<CollectionLog>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<DiscordMessage> DiscordMessages => Set<DiscordMessage>();
        public DbSet<GoodplaceShopItem> GoodplaceShopItems => Set<GoodplaceShopItem>();
        public DbSet<Competition> Competitions => Set<Competition>();
        public DbSet<GoodplaceSkillTask> GoodplaceSkillTasks => Set<GoodplaceSkillTask>();
        public DbSet<GoodplaceActivityTask> GoodplaceActivityTasks => Set<GoodplaceActivityTask>();
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => (
                optionsBuilder
                    .UseNpgsql()
                    .UseValidationCheckConstraints()
            )
                .EnableSensitiveDataLogging()
                .LogTo(
                    s =>
                    {
                        if (LoggingEnabled)
                        {
                            Console.WriteLine(s);
                        }
                }, LogLevel.Information);
        
    }
}
