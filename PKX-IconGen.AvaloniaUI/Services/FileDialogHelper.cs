using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                Filters = filters ?? new()
            };
            string[]? files = await fileDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            if (files != null)
            {
                string? file = files.FirstOrDefault();
                if (file != null)
                {
                    return file;
                }
            }
            return null;
        }

        public static async Task<string[]?> GetFiles(string title, List<FileDialogFilter>? filters = null)
        {
            OpenFileDialog fileDialog = new()
            {
                AllowMultiple = true,
                Title = title,
                Filters = filters ?? new()
            };
            string[]? files = await fileDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            if (files != null)
            {
                return files;
            }
            return null;
        }

        public static async Task<string?> GetFolder(string title)
        {
            OpenFolderDialog folderDialog = new()
            {
                Title = title
            };
            string? folder = await folderDialog.ShowAsync(Utils.GetApplicationLifetime().MainWindow);
            if (folder != null)
            {
                return folder;
            }
            return null;
        }
    }
}
