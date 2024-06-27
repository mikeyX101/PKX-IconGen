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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PKXIconGen.AvaloniaUI.Models;
using PKXIconGen.AvaloniaUI.Services;

namespace PKXIconGen.AvaloniaUI.Views.Controls;

public partial class FileTextField : UserControl
{
    private readonly TextBox fileTextBox;
    private readonly Button insertAssetsPathButton;
    private readonly Button browseFilesButton;

    private readonly List<FilePickerFileType> filters;
        
    public FileTextField()
    {
        InitializeComponent();

        fileTextBox = this.GetControl<TextBox>("FileTextBox");
            
        insertAssetsPathButton = this.GetControl<Button>("InsertAssetsPathButton");
        insertAssetsPathButton.Click += InsertAssetsPath;
            
        browseFilesButton = this.GetControl<Button>("BrowseFilesButton");
        browseFilesButton.Click += BrowseFiles;

        filters = new List<FilePickerFileType>();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsAssetsPathFieldProperty)
        {
            Grid.SetColumnSpan(fileTextBox, IsAssetsPathField ? 1 : 2);
        }
        else if (change.Property == TypeProperty)
        {
            filters.Clear();
            List<string> extensions = Type switch
            {
                FileSelectType.GCNModel => [".dat", ".pkx"],
                FileSelectType.Directory or FileSelectType.Executable or _ => ["*"]
            };
            if (OperatingSystem.IsWindows() && Type == FileSelectType.Executable)
            {
                extensions.Add(".exe");
            }
                
            string name = Type switch
            {
                FileSelectType.GCNModel => "GCN Pokemon Model",
                FileSelectType.Executable => "Executable",
                FileSelectType.Directory => "Folder",
                _ => ""
            };
                
            filters.Add(new FilePickerFileType(name) { Patterns = extensions });
        }
    }

    private void InsertAssetsPath(object? sender, RoutedEventArgs e) 
    {
        const string toInsert = "{{AssetsPath}}";
        if (fileTextBox.Text?.Contains(toInsert) ?? false) return;
            
        fileTextBox.Text = fileTextBox.Text is null ? toInsert : fileTextBox.Text.Insert(fileTextBox.CaretIndex, toInsert);
        fileTextBox.Focus();
        fileTextBox.CaretIndex += toInsert.Length;
    }
        
    private async void BrowseFiles(object? sender, RoutedEventArgs e)
    {
        IStorageItem? newItem = await OpenDialog();
        if (newItem == null) 
            return;
        
        string newPath = newItem.Path.LocalPath;
        if (AssetsPath != "" && newPath.Contains(AssetsPath))
        {
            newPath = newPath.Replace(AssetsPath, "{{AssetsPath}}");
        }
            
        Path = newPath;
    }
        
    private async Task<IStorageItem?> OpenDialog()
    {
        string? initialDirectory = !string.IsNullOrWhiteSpace(AssetsPath) && (string.IsNullOrWhiteSpace(Path) || Path.StartsWith("{{AssetsPath}}")) ? AssetsPath + '/' : Path;
        return Type switch
        {
            FileSelectType.Directory => await FileDialogHelper.GetFolder(Title, initialDirectory),
            FileSelectType.GCNModel or FileSelectType.Executable => await FileDialogHelper.GetFile(Title, filters, initialDirectory),
            _ => throw new InvalidOperationException("Unknown FileSelectType, somehow?")
        };
    }
}