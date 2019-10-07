using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiraiNotes.Core.Entities;

namespace MiraiNotes.Shared.EntitiesConfiguration
{
    public class GoogleTaskListConfiguration : IEntityTypeConfiguration<GoogleTaskList>
    {
        public void Configure(EntityTypeBuilder<GoogleTaskList> builder)
        {
            builder.HasKey(b => b.ID);
            builder.HasIndex(b => b.GoogleTaskListID).IsUnique();
            builder.Property(b => b.GoogleTaskListID).IsRequired();
            builder.Property(b => b.Title).IsRequired();
        }
    }
}
