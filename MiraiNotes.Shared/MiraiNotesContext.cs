using Microsoft.EntityFrameworkCore;
using MiraiNotes.Core.Entities;
using MiraiNotes.Shared.EntitiesConfiguration;
using System;
using System.IO;

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
            //If you need to create a migration, uncomment this line, and comment the above ones
            //Also, unload the uwp / android projects
            //optionsBuilder.UseSqlite($"Data Source=mirai-notes.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoogleTaskConfiguration).Assembly);
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