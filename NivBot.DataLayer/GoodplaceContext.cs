using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Npgsql;
using NivBot.DataLayer.Models;
using NivBot.DataLayer.TypeConfiguration;
using Microsoft.EntityFrameworkCore.Design;

namespace NivBot.DataLayer
{
    public class GoodplaceContext : DbContext
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
        public DbSet<CompetitionProviderDetails> CompetitionProvidersDetails => Set<CompetitionProviderDetails>();
        public DbSet<GoodplaceSkillTask> GoodplaceSkillTasks => Set<GoodplaceSkillTask>();
        public DbSet<GoodplaceActivityTask> GoodplaceActivityTasks => Set<GoodplaceActivityTask>();
        public DbSet<RunescapeStat> RunescapeStats => Set<RunescapeStat>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<ActivityCompetition> ActivityCompetitions => Set<ActivityCompetition>();
        public DbSet<SkillCompetition> SkillCompetitions => Set<SkillCompetition>();
        public DbSet<GlobalActivityBlocklist> GlobalActivityBlockLists => Set<GlobalActivityBlocklist>();
        public DbSet<ActivityTaskBlocklist> ActivityTaskBlockLists => Set<ActivityTaskBlocklist>();
        public DbSet<SkillTaskBlocklist> SkillTaskBlockLists => Set<SkillTaskBlocklist>();
        public DbSet<DiscordRole> DiscordRoles => Set<DiscordRole>();
        public DbSet<SkillRank> SkillRanks => Set<SkillRank>();
        public DbSet<ActivityRank> ActivityRanks => Set<ActivityRank>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodplaceContext).Assembly);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => (
                optionsBuilder
                    .UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres")
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
