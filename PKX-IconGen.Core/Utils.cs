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
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PKXIconGen.Core;

public static class Utils
{
    public static string CleanModelPathString(string modelPath)
    {
        char[] invalidChars = Path.GetInvalidPathChars();
        modelPath = invalidChars
            .Aggregate(modelPath, (current, invalidChar) => current.Replace(invalidChar.ToString(), ""));

        modelPath = modelPath.Replace("\"", "");
        return modelPath;
    }
        
    public static float ConvertRange(
        int originalStart, int originalEnd, // original range
        float newStart, float newEnd, // desired range
        int value) // value to convert
    {
        if (originalStart > value) {
            throw new ArgumentException("Value was smaller than the original range.", nameof(value));
        }
        if (originalEnd < value)
        {
            throw new ArgumentException("Value was greater than the original range.", nameof(value));
        }

        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return newStart + (value - originalStart) * scale;
    }

    public static float ConvertRange(
        uint originalStart, uint originalEnd, // original range
        float newStart, float newEnd, // desired range
        uint value) // value to convert
    {
        if (originalStart > value) {
            throw new ArgumentException("Value was smaller than the original range.", nameof(value));
        }
        if (originalEnd < value)
        {
            throw new ArgumentException("Value was greater than the original range.", nameof(value));
        }

        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return newStart + (value - originalStart) * scale;
    }

    public static float ConvertRange(
        float originalStart, float originalEnd, // original range
        float newStart, float newEnd, // desired range
        float value) // value to convert
    {
        if (originalStart > value) {
            throw new ArgumentException("Value was smaller than the original range.", nameof(value));
        }
        if (originalEnd < value)
        {
            throw new ArgumentException("Value was greater than the original range.", nameof(value));
        }

        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return newStart + (value - originalStart) * scale;
    }

    public static Task CleanTempFolders()
    {
        return Task.Run(() =>
        {
            IEnumerable<string> tempFiles = Directory.EnumerateFiles(Paths.TempFolder, "*", SearchOption.AllDirectories);
            IEnumerable<string> logFiles = Directory.EnumerateFiles(Paths.LogFolder, "*", SearchOption.AllDirectories);
            IEnumerable<string> files = tempFiles.Concat(logFiles).Where(f => f != Paths.Log);
                
            foreach (string file in files)
            {
                File.Delete(file);
            }
        });
    }
        
    public static Task CleanBlend1Files()
    {
        return Task.Run(() =>
        {
            IEnumerable<string> files = Directory.EnumerateFiles(Paths.TempBlendFolder, "*.blend1", SearchOption.TopDirectoryOnly);
                
            foreach (string file in files)
            {
                File.Delete(file);
            }
        });
    }

    public static string? GetTrueModelPath(string? model, string? assetsPath)
    {
        return model?.Replace("{{AssetsPath}}", assetsPath);
    }
        
    public static IDictionary<string, string> GetJsonPropNames<T>()
    {
        return typeof(T)
            .GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(JsonPropertyNameAttribute)).Count() == 1)
            .ToDictionary(
                p => p.Name,
                p => p.GetCustomAttributes(typeof(JsonPropertyNameAttribute)).OfType<JsonPropertyNameAttribute>().First().Name
            );
    }
}