using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data.Models;

namespace MiraiNotes.Data
{
    public class MiraiNotesContext : DbContext
    {
        public DbSet<GoogleUser> Users { get; set; }
        public DbSet<GoogleTaskList> TaskLists { get; set; }
        public DbSet<GoogleTask> Tasks { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=mirai-notes.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoogleUser>()
              .HasIndex(b => b.GoogleUserID)
              .IsUnique();

            modelBuilder.Entity<GoogleTaskList>()
              .HasIndex(b => b.GoogleTaskListID)
              .IsUnique();

            modelBuilder.Entity<GoogleTask>()
              .HasIndex(b => b.GoogleTaskID)
              .IsUnique();
        }

        public static void Init()
        {
            using(var context = new MiraiNotesContext())
            {
                context.Database.Migrate();
            }
        }
    }
}
