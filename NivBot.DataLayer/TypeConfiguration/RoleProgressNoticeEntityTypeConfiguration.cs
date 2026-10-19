using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NivBot.DataLayer.Models;

namespace NivBot.DataLayer.TypeConfiguration
{
    public class RoleProgressNoticeEntityTypeConfiguration : IEntityTypeConfiguration<RoleProgressNotice>
    {
        public void Configure(EntityTypeBuilder<RoleProgressNotice> builder)
        {
            builder
                .HasKey(x => new { x.GoodplaceUserId, x.DiscordRoleId });

            builder
                .HasOne(x => x.GoodplaceUser)
                .WithMany()
                .HasForeignKey(x => x.GoodplaceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.DiscordRole)
                .WithMany()
                .HasForeignKey(x => x.DiscordRoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
