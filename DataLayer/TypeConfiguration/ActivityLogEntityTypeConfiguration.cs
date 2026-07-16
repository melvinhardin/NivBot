using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;


namespace NivBot.DataLayer.TypeConfiguration
{
    public class ActivityLogEntityTypeConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder
                .HasKey(x => new {x.RunescapeAccountId,x.ActivityId});

            builder
                .HasOne<Activity>(x => x.Activity)
                .WithMany()
                .HasForeignKey(x => x.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.RunescapeAccount)
                .WithMany(x => x.ActivityLogs)
                .HasForeignKey(x => x.RunescapeAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .ToTable(t =>
                    {
                        t.HasCheckConstraint("CK_ActivityLog_Amount", "amount >= 0");
                    }
                );
            
        }
    }
}
