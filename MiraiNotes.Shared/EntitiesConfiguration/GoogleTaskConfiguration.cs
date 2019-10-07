using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiraiNotes.Core.Entities;
using MiraiNotes.Shared.Utils;

namespace MiraiNotes.Shared.EntitiesConfiguration
{
    public class GoogleTaskConfiguration : IEntityTypeConfiguration<GoogleTask>
    {
        public void Configure(EntityTypeBuilder<GoogleTask> builder)
        {
            builder.HasKey(b => b.ID);
            builder.HasIndex(b => b.GoogleTaskID).IsUnique();
            builder.Property(b => b.GoogleTaskID).IsRequired();
            builder.Property(b => b.Title).IsRequired();
            builder.Property(b => b.Status).IsRequired();
            builder.Property(b => b.Notes)
                .HasConversion(
                    note => EncryptUtil.EncryptString(note),
                    encriptedNote => EncryptUtil.DecryptString(encriptedNote));
        }
    }
}
