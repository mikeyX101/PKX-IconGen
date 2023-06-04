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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace PKXIconGen.AvaloniaUI.Services
{
    public static class FileDialogHelper
    {
        public static async Task<IStorageFile?> GetFile(string title, List<FilePickerFileType>? filters = null, string? initialDirectory = null, Window? callingWindow = null)
        {
            callingWindow ??= Utils.GetMainWindow();
            
            FilePickerOpenOptions dialogOptions = new()
            {
                AllowMultiple = false,
                Title = title,
                FileTypeFilter = filters,
                SuggestedStartLocation = initialDirectory is not null ? await callingWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory) : null
            };
            IReadOnlyList<IStorageFile> files = await callingWindow.StorageProvider.OpenFilePickerAsync(dialogOptions);
            return files.FirstOrDefault();
        }
        
        public static async Task<IReadOnlyList<IStorageFile>> GetFiles(string title, List<FilePickerFileType>? filters = null, string? initialDirectory = null, Window? callingWindow = null)
        {
            callingWindow ??= Utils.GetMainWindow();
            
            FilePickerOpenOptions dialogOptions = new()
            {
                AllowMultiple = true,
                Title = title,
                FileTypeFilter = filters,
                SuggestedStartLocation = initialDirectory is not null ? await callingWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory) : null
            };
            IReadOnlyList<IStorageFile> files = await callingWindow.StorageProvider.OpenFilePickerAsync(dialogOptions);
            return files;
        }

        public static async Task<IStorageFile?> SaveFile(string title, List<FilePickerFileType>? filters = null, string? initialFileName = null, string? defaultExtension = null, string? initialDirectory = null, Window? callingWindow = null)
        {
            callingWindow ??= Utils.GetMainWindow();
            
            FilePickerSaveOptions dialogOptions = new()
            {
                Title = title,
                FileTypeChoices = filters, 
                SuggestedFileName = initialFileName,
                DefaultExtension = defaultExtension,
                SuggestedStartLocation = initialDirectory is not null ? await callingWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory) : null,
                ShowOverwritePrompt = true
            };
            return await callingWindow.StorageProvider.SaveFilePickerAsync(dialogOptions);
        }
        
        public static async Task<IStorageFolder?> GetFolder(string title, string? initialDirectory = null, Window? callingWindow = null)
        {
            callingWindow ??= Utils.GetMainWindow();
            
            FolderPickerOpenOptions dialogOptions = new()
            {
                AllowMultiple = false,
                Title = title,
                SuggestedStartLocation = initialDirectory is not null ? await callingWindow.StorageProvider.TryGetFolderFromPathAsync(initialDirectory) : null
            };
            IReadOnlyList<IStorageFolder> folders = await callingWindow.StorageProvider.OpenFolderPickerAsync(dialogOptions);
            return folders.FirstOrDefault();
        }
    }
}
