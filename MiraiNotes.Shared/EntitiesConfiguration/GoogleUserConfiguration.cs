using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiraiNotes.Core.Entities;

namespace MiraiNotes.Shared.EntitiesConfiguration
{
    public class GoogleUserConfiguration : IEntityTypeConfiguration<GoogleUser>
    {
        public void Configure(EntityTypeBuilder<GoogleUser> builder)
        {
            builder.HasKey(b => b.ID);
            builder.HasIndex(b => b.GoogleUserID).IsUnique();
            builder.Property(b => b.GoogleUserID).IsRequired();
            builder.Property(b => b.Fullname).IsRequired();
            builder.Property(b => b.Email).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();
        }
    }
}
