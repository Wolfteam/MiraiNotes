using Microsoft.EntityFrameworkCore;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Entities;
using MiraiNotes.Shared.EntitiesConfiguration;
using Serilog;
using System;
using System.IO;

namespace MiraiNotes.Shared
{
    public class MiraiNotesContext : DbContext
    {
        private const string DatabaseName = "mirai-notes.db";
        private const string CurrentMigration = "Migration_v1.0.0.0";


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

        public static void Init(IAppSettingsService appSettings, ILogger logger)
        {
            logger.Information($"Checking if the lastest migration = {CurrentMigration} is applied");
            if (appSettings.CurrentAppMigration == CurrentMigration)
            {
                logger.Information("Lastest migration is applied...");
                return;
            }

            logger.Information("Migration is not applied... Aplying it...");
            using (var context = new MiraiNotesContext())
            {
                context.Database.Migrate();

                appSettings.CurrentAppMigration = CurrentMigration;
            }
            logger.Information("Migration was successfully applied");
        }
    }
}