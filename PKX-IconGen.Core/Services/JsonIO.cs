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

using PKXIconGen.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Services
{
    /// <summary>
    /// IO for Json.
    /// Try/Catch the functions to handle errors.
    /// </summary>
    public static class JsonIO
    {
        public static readonly JsonSerializerOptions defaultOptions = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };

        public static async Task ExportAsync<T>(T data, string path) where T : IJsonSerializable
        {
            try
            {
                using FileStream file = File.Create(path);
                await JsonSerializer.SerializeAsync(file, data, defaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while serializing to JSON in file: {@FilePath}. Object getting serialized: {@Object}", path, data);
                throw;
            }
        }

        public static async Task<T?> ImportAsync<T>(string path) where T : IJsonSerializable
        {
            try
            {
                using FileStream file = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<T>(file, defaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while deserializing from JSON in file: {@FilePath}.", path);
                throw;
            }
        }

        public static IAsyncEnumerable<T?>? ImportAsyncEnumerable<T>(string path) where T : IJsonSerializable
        {
            try
            {
                using FileStream file = File.OpenRead(path);
                return JsonSerializer.DeserializeAsyncEnumerable<T>(file, defaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while getting the AsyncEnumerable from JSON in file: {@FilePath}.", path);
                throw;
            }
        }

        public async static IAsyncEnumerable<T?> ImportAsyncEnumerable<T>(string[] paths) where T : IJsonSerializable?
        {
            foreach (string path in paths)
            {
                using FileStream file = File.OpenRead(path);
                yield return await JsonSerializer.DeserializeAsync<T>(file, defaultOptions);
            }
        }
    }
}
