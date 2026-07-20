using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;


namespace NivBot.DataLayer.TypeConfiguration
{
    public class CollectionLogEntityTypeConfiguration : IEntityTypeConfiguration<CollectionLog>
    {
        public void Configure(EntityTypeBuilder<CollectionLog> builder)
        {
            

            builder
                .HasKey(x => new {x.RunescapeAccountId,x.ItemId});

            builder
                .HasOne<Item>(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<RunescapeAccount>(x => x.RunescapeAccount)
                .WithMany(x => x.CollectionLogs)
                .HasForeignKey(x => x.RunescapeAccountId)
                .OnDelete(DeleteBehavior.Cascade);


            builder
                .ToTable(t =>
                    {
                        t.HasCheckConstraint("CK_CollectionLog_Amount", "\"Amount\" >= 0");
                    }
                );
            
        }
    }
}
