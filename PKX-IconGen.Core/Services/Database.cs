using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PKXIconGen.Core.Data;

namespace PKXIconGen.Core.Services
{
    public class Database : DbContext
    {
        private const uint SettingsId = 1;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dataDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                
                if (!Directory.Exists(dataDirectoryPath))
                {
                    Directory.CreateDirectory(dataDirectoryPath);
                }
                optionsBuilder.UseSqlite($"Data Source={dataDirectoryPath}/DB.db;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>().HasData(new Settings());
        }

        public bool RunMigrations()
        {
            try
            {
                CoreManager.Logger.Information("Running Database migrations...");
                Database.Migrate();
                CoreManager.Logger.Information("Database migration successful");
                return true;
            }
            catch (Exception e)
            {
                CoreManager.Logger.Error(e, "Database migration failed");
                return false;
            }
        }

        public DbSet<Settings>? SettingsTable { get; set; }

        #region Settings
        public Settings GetSettings()
        {
            if (SettingsTable != null)
            {
                return SettingsTable.FirstOrDefault() ?? new Settings();
            }
            else
            {
                throw new InvalidOperationException("SettingsTable was somehow null?");
            }
        }

        public void SaveSettings(Settings settings)
        {
            if (SettingsTable != null)
            {
                SettingsTable.Update(settings);
            }
            else
            {
                throw new InvalidOperationException("SettingsTable was somehow null?");
            }
        }

        public void SaveSettingsProperty<TProperty>(Expression<Func<Settings, TProperty>> propertySelector, TProperty value)
        {
            if (SettingsTable != null)
            {
                Settings settings = SettingsTable
                    .Where(s => s.InternalID == SettingsId)
                    .First();

                EntityEntry<Settings> entity = SettingsTable.Attach(settings);
                entity.Property(propertySelector).CurrentValue = value;

                SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("SettingsTable was somehow null?");
            }
        }
        #endregion
    }
}
