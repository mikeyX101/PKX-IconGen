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

namespace PKXIconGen.Core
{
    internal static class Paths
    {
        // Data Folder
        internal static string DataFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        
        // Logs
        internal static string LogFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static string SessionLog { get; } = $"log-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
        internal static string Log => Path.Combine(LogFolder, SessionLog);

        internal static string LogLatest => Path.Combine(LogFolder, $"latest.log");

        internal static string BlenderLogsFolder => Path.Combine(LogFolder, "Blender");
        internal static string BlenderLog => Path.Combine(BlenderLogsFolder, $"blenderLog-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        // Temp Folders
        internal static string TempFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        internal static string TempBlendFolder => Path.Combine(TempFolder, "Blend");

        
        // Python Folders
        internal static string PythonFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python");

        // Python Files
        internal static string Render => Path.Combine(PythonFolder, "render.py");
        internal static string ModifyData => Path.Combine(PythonFolder, "modify_data.py");

        // Template
        private static string Template => Path.Combine(PythonFolder, "template.blend");
        internal static string GetTemplateCopy(string templateName) 
        {
            string copy = Path.Combine(TempBlendFolder, $"{templateName}.blend");
            File.Copy(Template, copy, true);
            return copy;
        }
    }
}
