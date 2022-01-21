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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Blender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Services
{
    public class Database : DbContext
    {
        private static Database? instance;
        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Database();
                }
                return instance;
            }
        }
        internal static void OnClose()
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }
        }

        private const uint SettingsId = 1;

        private static Exception IfNullTable(string propertyName)
        {
            Exception ex = new InvalidOperationException($"{propertyName} was somehow null?");
            CoreManager.Logger.Fatal(ex, "{TableName} was somehow null?", propertyName);
            return ex;
        }

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

            JsonSerializerOptions? serializerOptions = null;
            EntityTypeBuilder<PokemonRenderData> pokemonRenderDataEntityBuilder = modelBuilder.Entity<PokemonRenderData>();
            pokemonRenderDataEntityBuilder.Property(e => e.Shiny)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<ShinyInfo>(v, serializerOptions));

            pokemonRenderDataEntityBuilder.Property(e => e.MainCamera)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<Camera>(v, serializerOptions));
            pokemonRenderDataEntityBuilder.Property(e => e.SecondaryCamera)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<Camera>(v, serializerOptions));

            pokemonRenderDataEntityBuilder.Property(e => e.MainLights)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<Light[]>(v, serializerOptions) ?? Array.Empty<Light>(),
                    new ValueComparer<Light[]>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()));
            pokemonRenderDataEntityBuilder.Property(e => e.SecondaryLights)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializerOptions),
                    v => JsonSerializer.Deserialize<Light[]>(v, serializerOptions) ?? Array.Empty<Light>(),
                    new ValueComparer<Light[]>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()));
        }

        public void RunMigrations()
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

        public DbSet<Settings>? SettingsTable { get; set; }
        public DbSet<PokemonRenderData>? PokemonRenderDataTable { get; set; }

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
                Settings settings = SettingsTable
                    .Where(s => s.InternalID == SettingsId)
                    .First();

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

                return SaveChanges();
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
                PokemonRenderData pokemonRenderData = PokemonRenderDataTable
                    .Where(prd => prd.InternalID == id && !prd.BuiltIn)
                    .First();

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
