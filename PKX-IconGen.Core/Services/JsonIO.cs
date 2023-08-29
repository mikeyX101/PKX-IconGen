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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Compatibility;

namespace PKXIconGen.Core.Services
{
    /// <summary>
    /// IO for Json.
    /// Try/Catch the functions to handle errors.
    /// </summary>
    public static class JsonIO
    {
        public static readonly JsonSerializerOptions DefaultOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static async Task ExportAsync<T>(T data, string path) where T : IJsonSerializable
        {
            try
            {
                await using FileStream file = File.Create(path);
                await JsonSerializer.SerializeAsync(file, data, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while serializing to JSON in file: {@FilePath}. Object getting serialized: {@Object}", path, data);
                throw;
            }
        }
        
        public static async Task ExportAsync<T>(T data, Stream stream) where T : IJsonSerializable
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, data, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while serializing to JSON in stream. Object getting serialized: {@Object}", data);
                throw;
            }
        }

        public static async Task<T?> ImportAsync<T>(string path) where T : IJsonSerializable
        {
            try
            {
                await using FileStream file = File.OpenRead(path);
                return await JsonSerializer.DeserializeAsync<T>(file, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while deserializing from JSON in file: {@FilePath}", path);
                throw;
            }
        }
        
        public static async Task<T?> ImportAsync<T>(Stream stream) where T : IJsonSerializable
        {
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while deserializing from JSON in stream");
                throw;
            }
        }

        public static IAsyncEnumerable<T?> ImportAsyncEnumerable<T>(string path) where T : IJsonSerializable
        {
            try
            {
                using FileStream file = File.OpenRead(path);
                return JsonSerializer.DeserializeAsyncEnumerable<T>(file, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while getting the AsyncEnumerable from JSON in file: {@FilePath}", path);
                throw;
            }
        }
        
        public static IAsyncEnumerable<T?> ImportAsyncEnumerable<T>(Stream stream) where T : IJsonSerializable
        {
            try
            {
                return JsonSerializer.DeserializeAsyncEnumerable<T>(stream, DefaultOptions);
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while getting the AsyncEnumerable from JSON in stream");
                throw;
            }
        }

        public static async IAsyncEnumerable<T?> ImportAsyncEnumerable<T>(IEnumerable<string> paths) where T : IJsonSerializable?
        {
            foreach (string path in paths)
            {
                await using FileStream file = File.OpenRead(path);
                yield return await JsonSerializer.DeserializeAsync<T>(file, DefaultOptions);
            }
        }
        
        public static async IAsyncEnumerable<T?> ImportAsyncEnumerable<T>(IEnumerable<Stream> streams) where T : IJsonSerializable?
        {
            foreach (Stream stream in streams)
            {
                yield return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions);
            }
        }

        public static string ToJsonString<T>(T data) where T : IJsonSerializable
        {
            try
            {
                using MemoryStream memoryStream = new(2048);
                JsonSerializer.Serialize(memoryStream, data, DefaultOptions);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while serializing to JSON in a string. Object getting serialized: {@Object}", data);
                throw;
            }
        }

        public static async Task<string> ToJsonStringAsync<T>(T data) where T : IJsonSerializable
        {
            try
            {
                using MemoryStream memoryStream = new(2048);
                await JsonSerializer.SerializeAsync(memoryStream, data, DefaultOptions);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                CoreManager.Logger.Error(ex, "Error while serializing to JSON in a string. Object getting serialized: {@Object}", data);
                throw;
            }
        }
    }
}
