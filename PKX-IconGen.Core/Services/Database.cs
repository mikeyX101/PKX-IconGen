#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PKXIconGen.Core.Data;

namespace PKXIconGen.Core.Services
{
    public sealed class Database : DbContext
    {
        [UsedImplicitly]
        internal class DatabaseDesignTimeFactory : IDesignTimeDbContextFactory<Database>
        {
            public Database CreateDbContext(string[] args) => new();
        }

        private static Database? _instance;
        public static Database Instance
        {
            get
            {
                if (_instance == null || _instance.Disposed)
                {
                    _instance = new Database();
                }
                return _instance;
            }
        }
        internal static void OnClose()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        private const uint SettingsId = 1;

        private Database() : base()
        {
            
        }

        private static Exception IfNullTable(string propertyName)
        {
            Exception ex = new InvalidOperationException($"{propertyName} was somehow null?");
            CoreManager.Logger.Fatal(ex, "{@TableName} was somehow null?", propertyName);
            return ex;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (!Directory.Exists(Paths.DataFolder))
                {
                    Directory.CreateDirectory(Paths.DataFolder);
                }
                optionsBuilder.UseSqlite($"Data Source={Paths.DataFolder}/DB.db;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Settings>().HasData(new Settings());

            JsonSerializerOptions? serializerOptions = null;
            EntityTypeBuilder<PokemonRenderData> pokemonRenderDataEntityBuilder = modelBuilder.Entity<PokemonRenderData>();

            pokemonRenderDataEntityBuilder.Property(nameof(PokemonRenderData.OutputName)).IsRequired(false);

            pokemonRenderDataEntityBuilder.Property<RenderData>(nameof(PokemonRenderData.Render))
                .HasDefaultValue(new RenderData())
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<RenderData>(v, serializerOptions) ?? new RenderData());

            pokemonRenderDataEntityBuilder.Property<ShinyInfo>(nameof(PokemonRenderData.Shiny))
                .HasDefaultValue(new ShinyInfo())
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<ShinyInfo>(v, serializerOptions) ?? new ShinyInfo());
        }

        internal void RunMigrations()
        {
            try
            {
                CoreManager.Logger.Information("Running Database migrations...");
                Database.Migrate();
                CoreManager.Logger.Information("Database migration successful");
            }
            catch (Exception e)
            {
                CoreManager.Logger.Error(e, "Database migration failed");
                throw;
            }
        }

        private bool Disposed { get; set; }
        public override void Dispose()
        {
            Disposed = true;
            base.Dispose();
        }

        public DbSet<Settings>? SettingsTable { get; set; }
        public DbSet<PokemonRenderData>? PokemonRenderDataTable { get; set; }

        //https://stackoverflow.com/questions/16437083/dbcontext-discard-changes-without-disposing/22098063#22098063
        public void RejectChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        #region Settings
        public async Task<Settings> GetSettingsAsync()
        {
            if (SettingsTable != null)
            {
                return await SettingsTable.FirstOrDefaultAsync() ?? new Settings();
            }
            else
            {
                throw IfNullTable(nameof(SettingsTable));
            }
        }

        public void SaveSettingsProperty<TProperty>(Expression<Func<Settings, TProperty>> propertySelector, TProperty value)
        {
            if (SettingsTable != null)
            {
                Settings settings = SettingsTable.First(s => s.InternalID == SettingsId);

                EntityEntry<Settings> entity = SettingsTable.Attach(settings);
                entity.Property(propertySelector).CurrentValue = value;

                SaveChanges();
            }
            else
            {
                throw IfNullTable(nameof(SettingsTable));
            }
        }
        #endregion

        #region Pokemon Render Data
        public async Task<int> AddPokemonRenderDataAsync(PokemonRenderData pokemonRenderData)
        {
            if (PokemonRenderDataTable != null)
            {
                await PokemonRenderDataTable.AddAsync(pokemonRenderData);

                return await SaveChangesAsync();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        public async Task<int> UpdatePokemonRenderDataAsync(PokemonRenderData newData)
        {
            if (PokemonRenderDataTable != null)
            {
                EntityEntry<PokemonRenderData> dataEntry = PokemonRenderDataTable.Update(newData);
                dataEntry.State = EntityState.Modified;
                return await SaveChangesAsync();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        private List<PokemonRenderData>? BuiltInPRDs;
        public List<PokemonRenderData> GetPokemonRenderDataBuiltIns()
        {
            if (BuiltInPRDs != null)
            {
                return BuiltInPRDs;
            }

            if (PokemonRenderDataTable != null)
            {
                BuiltInPRDs = PokemonRenderDataTable.Where(prd => prd.BuiltIn).ToList();
                return BuiltInPRDs;
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        public async Task<List<PokemonRenderData>> GetPokemonRenderDataAsync(bool orderedByName = true)
        {
            if (PokemonRenderDataTable != null)
            {
                List<PokemonRenderData> data = await PokemonRenderDataTable.ToListAsync();
                return orderedByName ? data.OrderBy(prd => prd.Name).ToList() : data;
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        public async Task<int> DeletePokemonRenderDataAsync(uint id)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderData pokemonRenderData = PokemonRenderDataTable.First(prd => prd.Id == id && !prd.BuiltIn);

                PokemonRenderDataTable.RemoveRange(pokemonRenderData);

                return await SaveChangesAsync();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        public async Task<int> DeletePokemonRenderDataAsync(PokemonRenderData renderData)
        {
            if (PokemonRenderDataTable != null)
            {
                if (!renderData.BuiltIn)
                {
                    PokemonRenderDataTable.Remove(renderData);
                }
                return await SaveChangesAsync();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        public async Task<int> DeletePokemonRenderDataAsync(IEnumerable<PokemonRenderData> renderData)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderDataTable.RemoveRange(renderData.Where(prd => !prd.BuiltIn));

                return await SaveChangesAsync();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        #endregion
    }
}
