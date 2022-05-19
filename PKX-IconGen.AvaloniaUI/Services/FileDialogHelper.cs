#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
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

using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.Services
{
    public static class FileDialogHelper
    {
        public static async Task<string?> GetFile(string title, List<FileDialogFilter>? filters = null)
        {
            OpenFileDialog fileDialog = new()
            {
                AllowMultiple = false,
                Title = title,
                Filters = filters ?? new List<FileDialogFilter>()
            };
            string[]? files = await fileDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            string? file = files?.FirstOrDefault();
            return file;
        }

        public static async Task<string?> SaveFile(string title, List<FileDialogFilter>? filters = null, string? initialFileName = null)
        {
            SaveFileDialog fileDialog = new()
            {
                Title = title,
                Filters = filters ?? new List<FileDialogFilter>(),
                InitialFileName = initialFileName
            };
            string? file = await fileDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            return file;
        }

        public static async Task<string[]?> GetFiles(string title, List<FileDialogFilter>? filters = null)
        {
            OpenFileDialog fileDialog = new()
            {
                AllowMultiple = true,
                Title = title,
                Filters = filters ?? new List<FileDialogFilter>()
            };
            string[]? files = await fileDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            return files;
        }

        public static async Task<string?> GetFolder(string title)
        {
            OpenFolderDialog folderDialog = new()
            {
                Title = title
            };
            string? folder = await folderDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            return folder;
        }
    }
}
