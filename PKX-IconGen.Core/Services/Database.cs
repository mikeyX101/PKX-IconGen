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
using LinqToDB;
using LinqToDB.Linq;
using LinqToDB.EntityFrameworkCore;
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

        internal Task GetMigrationTask()
        {
            return Task.Run(async () =>
            {
                try
                {
                    CoreManager.Logger.Information("Running Database migrations...");
                    await Database.MigrateAsync();
                    CoreManager.Logger.Information("Database migration successful");
                }
                catch (Exception e)
                {
                    CoreManager.Logger.Error(e, "Database migration failed");
                    throw;
                }
            });
        }

        private bool Disposed { get; set; }
        public override void Dispose()
        {
            Disposed = true;
            base.Dispose();
        }

        [UsedImplicitly]
        public DbSet<Settings>? SettingsTable { get; internal set; }
        private IQueryable<Settings>? SettingsLinq => SettingsTable?.ToLinqToDB();
        
        [UsedImplicitly]
        public DbSet<PokemonRenderData>? PokemonRenderDataTable { get; internal set; }
        private IQueryable<PokemonRenderData>? PokemonRenderDataLinq => PokemonRenderDataTable?.ToLinqToDB();

        #region Settings
        public async Task<Settings> GetSettingsAsync()
        {
            if (SettingsLinq != null)
            {
                return await SettingsLinq.FirstOrDefaultAsyncLinqToDB() ?? new Settings();
            }
            
            throw IfNullTable(nameof(SettingsLinq));
        }

        public int SaveSettingsProperty<TProperty>(Expression<Func<Settings, TProperty>> propertySelector, TProperty value)
        {
            if (SettingsLinq != null)
            {
                return SettingsLinq
                    .Where(s => s.InternalID == SettingsId)
                    .Set(propertySelector, value)
                    .Update();
            }
            
            throw IfNullTable(nameof(SettingsLinq));
        }
        #endregion

        #region Pokemon Render Data
        public async Task<int> AddPokemonRenderDataAsync(PokemonRenderData prd)
        {
            if (PokemonRenderDataLinq != null)
            {
                await using IDataContext ctx = this.CreateLinqToDbContext();

                int id = await ctx.InsertWithInt32IdentityAsync(prd);
                if (id > 0)
                {
                    prd.Id = (uint)id;
                    return 1;
                }

                return 0;
            }
            
            throw IfNullTable(nameof(PokemonRenderDataLinq));
        }

        public async Task<int> UpdatePokemonRenderDataAsync(PokemonRenderData prd)
        {
            if (PokemonRenderDataTable != null)
            {
                await using IDataContext ctx = this.CreateLinqToDbContext();
                return await ctx.UpdateAsync(prd);
            }
            
            throw IfNullTable(nameof(PokemonRenderDataTable));
        }

        public async Task<List<PokemonRenderData>> GetPokemonRenderDataAsync(bool orderedByName = true)
        {
            if (PokemonRenderDataLinq != null)
            {
                List<PokemonRenderData> data = await PokemonRenderDataLinq.ToListAsyncLinqToDB();
                return orderedByName ? data.OrderBy(prd => prd.Name).ToList() : data;
            } 
            
            throw IfNullTable(nameof(PokemonRenderDataLinq));
        }
        
        public async Task<int> DeletePokemonRenderDataAsync(IEnumerable<PokemonRenderData> prds)
        {
            if (PokemonRenderDataLinq != null)
            {
                uint[] prdIds = prds.Select(prd => prd.Id).ToArray();

                return await PokemonRenderDataLinq
                    .Where(prd => prdIds.Contains(prd.Id))
                    .DeleteAsync();
            }
            
            throw IfNullTable(nameof(PokemonRenderDataLinq));
        }
        #endregion
    }
}
