using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Npgsql;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.TypeConfiguration;

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
        public DbSet<CompetitionProviderDetails> CompetitionProvidersDetails => Set<CompetitionProviderDetails>();
        public DbSet<GoodplaceSkillTask> GoodplaceSkillTasks => Set<GoodplaceSkillTask>();
        public DbSet<GoodplaceActivityTask> GoodplaceActivityTasks => Set<GoodplaceActivityTask>();
        public DbSet<RunescapeStat> RunescapeStats => Set<RunescapeStat>();
        public DbSet<Skill> SkillEntities => Set<Skill>();
        public DbSet<ActivityCompetition> ActivityCompetitions => Set<ActivityCompetition>();
        public DbSet<SkillCompetition> SkillCompetitions => Set<SkillCompetition>();
        public DbSet<GlobalActivityBlocklist> GlobalActivityBlockLists => Set<GlobalActivityBlocklist>();
        public DbSet<ActivityTaskBlocklist> ActivityTaskBlockLists => Set<ActivityTaskBlocklist>();
        public DbSet<SkillTaskBlocklist> SkillTaskBlockLists => Set<SkillTaskBlocklist>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityBlocklistEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityCompetitionEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityLogEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollectionLogEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompetitionProviderDetailsEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiscordMessageEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GlobalActivityBlocklistEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceActivityTaskEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceShopItemEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceSkillTaskEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceUserEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceShopItemEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RunescapeAccountEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RunescapeStatEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkillBlocklistEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkillCompetitionEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SkillEntityTypeConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletEntityTypeConfiguration).Assembly);

        }
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
