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
        public DbSet<CompetitionProviderDetails> CompetitionProviderDetails => Set<CompetitionProviderDetails>();
        public DbSet<GoodplaceSkillTask> GoodplaceSkillTasks => Set<GoodplaceSkillTask>();
        public DbSet<GoodplaceActivityTask> GoodplaceActivityTasks => Set<GoodplaceActivityTask>();
        public DbSet<RunescapeStat> RunescapeStats => Set<RunescapeStat>();
        public DbSet<SkillEntity> SkillEntities => Set<SkillEntity>();
        public DbSet<ActivityCompetition> ActivityCompetitions => Set<ActivityCompetition>();
        public DbSet<SkillCompetition> SkillCompetitions => Set<SkillCompetition>();
        public DbSet<GlobalActivityBlocklist> GlobalActivityBlocks => Set<GlobalActivityBlocklist>();
        public DbSet<ActivityTaskBlocklist> ActivityTaskBlockLists => Set<ActivityTaskBlocklist>();
        public DbSet<SkillTaskBlocklist> SkillTaskBlockLists => Set<SkillTaskBlocklist>();

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
