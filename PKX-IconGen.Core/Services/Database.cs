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

namespace PKXIconGen.Core.Services
{
    public class Database : DbContext
    {
        private const uint SettingsId = 1;

        private static Exception IfNullTable(string propertyName)
        {
            Exception ex = new InvalidOperationException($"{propertyName} was somehow null?");
            CoreManager.Logger.Fatal(new InvalidOperationException($"{propertyName} was somehow null?"), "{TableName} was somehow null?", propertyName);
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

            EntityTypeBuilder<PokemonRenderData> pokemonRenderDataEntityBuilder = modelBuilder.Entity<PokemonRenderData>();
            pokemonRenderDataEntityBuilder.Property(e => e.Camera)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Camera>(v, (JsonSerializerOptions?)null));
            pokemonRenderDataEntityBuilder.Property(e => e.Lights)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Light[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<Light>(),
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
        public Settings GetSettings()
        {
            if (SettingsTable != null)
            {
                return SettingsTable.FirstOrDefault() ?? new Settings();
            }
            else
            {
                throw IfNullTable(nameof(SettingsTable));
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
        public int AddPokemonRenderData(PokemonRenderData pokemonRenderData)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderDataTable.Add(pokemonRenderData);

                return SaveChanges();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        public List<PokemonRenderData> GetPokemonRenderData()
        {
            if (PokemonRenderDataTable != null)
            {
                return PokemonRenderDataTable.ToList();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }

        public int DeletePokemonRenderData(uint id)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderData pokemonRenderData = PokemonRenderDataTable
                    .Where(prd => prd.InternalID == id && !prd.BuiltIn)
                    .First();

                PokemonRenderDataTable.RemoveRange(pokemonRenderData);

                return SaveChanges();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        public int DeletePokemonRenderData(PokemonRenderData renderData)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderDataTable.RemoveRange(renderData);

                return SaveChanges();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        public int DeletePokemonRenderData(IEnumerable<PokemonRenderData> renderData)
        {
            if (PokemonRenderDataTable != null)
            {
                PokemonRenderDataTable.RemoveRange(renderData);

                return SaveChanges();
            }
            else
            {
                throw IfNullTable(nameof(PokemonRenderDataTable));
            }
        }
        #endregion
    }
}
