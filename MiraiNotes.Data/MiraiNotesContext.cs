using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data.Models;

namespace MiraiNotes.Data
{
    public class MiraiNotesContext : DbContext
    {
        public DbSet<GoogleUser> Users { get; set; }
        public DbSet<GoogleTaskList> TaskLists { get; set; }
        public DbSet<GoogleTask> Tasks { get; set; }

        protected string DatabasePath { get; set; }

        //When creating a migration, uncomment this constructor
        private MiraiNotesContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=mirai-notes.db");
            //optionsBuilder.UseSqlite($"{DatabasePath}");
        }

        public static MiraiNotesContext Create(string databasePath)
        {
            var dbContext = new MiraiNotesContext(databasePath);
            dbContext.Database.Migrate();
            //dbContext.Database.EnsureCreated();
            return dbContext;
            //return null;
        }
    }
}
