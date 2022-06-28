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
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace PKXIconGen.Core.Services
{
    public readonly struct BlenderCheckResult
    {
        public readonly bool IsBlender { get; init; }

        public readonly bool IsValidVersion { get; init; }

        public readonly float Version { get; init; }

        public BlenderCheckResult(bool isBlender, bool isValidVersion, float version)
        {
            IsBlender = isBlender;
            IsValidVersion = isValidVersion;
            Version = version;
        }
    }

    public static class BlenderVersionChecker
    {
        private const float MinimumBlenderVersion = 2.93f;

        public static BlenderCheckResult? CheckExecutable(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!OperatingSystem.IsWindows())
                {
                    // Ask with "blender --version"?
                    return new(true, true, 0);
                }
                else if (File.Exists(path))
                {
                    // Windows has metadata we can use.
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                    string? name = fileVersionInfo.ProductName;
                    float? version = fileVersionInfo.ProductVersion != null ? float.Parse(fileVersionInfo.ProductVersion, CultureInfo.InvariantCulture) : null;

                    if (name != null && version.HasValue)
                    {
                        bool isBlender = name.Equals("Blender");
                        bool isValidVersion = version.Value >= MinimumBlenderVersion;
                        return new BlenderCheckResult(isBlender, isValidVersion, version.Value);    
                    }
                }
            }
            return null;
        }
    }
}
