using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MiraiNotes.Core.Entities;
using MiraiNotes.Shared.Utils;

namespace MiraiNotes.Shared
{
    public class MiraiNotesContext : DbContext
    {
        private const string DatabaseName = "mirai-notes.db";
        private const string CurrentMigration = "Migration_v1";


        public DbSet<GoogleUser> Users { get; set; }
        public DbSet<GoogleTaskList> TaskLists { get; set; }
        public DbSet<GoogleTask> Tasks { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = string.Empty;
            
#if Android
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DatabaseName);
#else
            databasePath = DatabaseName;
#endif
            optionsBuilder.UseSqlite($"Filename={databasePath}");

//            optionsBuilder.UseSqlite($"Data Source=mirai-notes.db");
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

            modelBuilder.Entity<GoogleTask>()
                .Property(b => b.Notes)
                .HasConversion(
                    note => EncryptUtil.EncryptString(note),
                    encriptedNote => EncryptUtil.DecryptString(encriptedNote));
        }

        public static void Init()
        {
            using (var context = new MiraiNotesContext())
            {
                context.Database.Migrate();
            }
        }
    }
}