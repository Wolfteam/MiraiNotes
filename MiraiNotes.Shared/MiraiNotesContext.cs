using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Entities;
using MiraiNotes.Shared.EntitiesConfiguration;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace MiraiNotes.Shared
{
    public class MiraiNotesContext : DbContext
    {
        private const string DatabaseName = "mirai-notes.db";
        private const string CurrentMigration = "Migration_v1.1.4.0";


        public DbSet<GoogleUser> Users { get; set; }
        public DbSet<GoogleTaskList> TaskLists { get; set; }
        public DbSet<GoogleTask> Tasks { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string databasePath = string.Empty;

#if Android
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), DatabaseName);
#else
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseName);
#endif
            optionsBuilder.UseSqlite($"Filename={databasePath}");
            //If you need to create a migration, uncomment this line, and comment the above ones
            //Also, unload the uwp / android projects
            //optionsBuilder.UseSqlite($"Data Source=mirai-notes.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoogleTaskConfiguration).Assembly);

            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                            || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }
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