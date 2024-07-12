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
using System.IO;
using System.Text;

namespace PKXIconGen.Core;

public static class Versions
{
    public const string ImporterCommit = "56192b582f6d07599f24eb0e0e48d1c6886d2ac9";
    public static DateTime ImporterDate => new(2021, 08, 27);

    public static string AddonVersion => GetAddonVersion();
    private static string? CachedAddonVersion;
    private static string GetAddonVersion()
    {
        if (CachedAddonVersion is not null)
        {
            return CachedAddonVersion;
        }

        try
        {
            using FileStream addonFile = File.Open(Path.Combine(Paths.PythonFolder, "version.py"), FileMode.Open, FileAccess.Read, FileShare.Read);
            using TextReader reader = new StreamReader(addonFile, Encoding.UTF8);
            string? versionLine = reader.ReadLine() ?? throw new FormatException("Version file format is invalid");
            CachedAddonVersion = versionLine.Replace("addon_ver_str: str = \"", "").Replace("\"", "");
        }
        catch (Exception e)
        {
            PKXCore.Logger.Error(e, "Error while getting addon version");
            CachedAddonVersion = "Unknown";
        }

        return CachedAddonVersion;
    }
}