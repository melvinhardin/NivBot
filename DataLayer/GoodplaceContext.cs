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
        public DbSet<Collectable> Collectables => Set<Collectable>();
        public DbSet<Boss> Bosses => Set<Boss>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Raid> Raids => Set<Raid>();
        

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
