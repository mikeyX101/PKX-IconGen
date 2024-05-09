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
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JetBrains.Annotations;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow { get; init; }

        public MenuViewModel(MainWindowViewModel mainWindow)
        {
            MainWindow = mainWindow;

            // Reactive 
            IObservable<bool> exportEnabled = this.WhenAnyValue(
                vm => vm.MainWindow.NbOfRenders,
                nbOfRenders => nbOfRenders > 0);

            ExportCommand = ReactiveCommand.CreateFromTask(Export, exportEnabled);
            ExportBlenderCommand = ReactiveCommand.CreateFromTask(ExportBlender, exportEnabled);
            OpenSettingsCommand = ReactiveCommand.CreateFromTask(OpenSettings);
        }

        [UsedImplicitly]
        public async void Import()
        {
            List<string> extensions = new() { "*.json" };
            List<FilePickerFileType> filters = new()
            {
                new FilePickerFileType("PKX-IconGen Json") { Patterns = extensions }
            };
            
            IReadOnlyList<IStorageFile> files = await FileDialogHelper.GetFiles("Select Json files to import...", filters);
            try
            {
                foreach (IStorageFile file in files)
                {
                    await using Stream fileStream = await file.OpenReadAsync();
                    OnImport?.Invoke(await JsonIO.ImportAsync<PokemonRenderData>(fileStream));
                }
            }
            catch (Exception ex) when (ex is ArgumentException or JsonException)
            {
                CoreManager.Logger.Error(ex, "An exception occured while importing data. Json is probably invalid");
                await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "An error occured while importing. The given Json is invalid.\nClose the application and see the logs for further details.");
            }
            catch (Exception ex) 
            {
                CoreManager.Logger.Error(ex, "An unexpected exception occured while importing data");
                await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "An unexpected error occured while importing. \nClose the application and see the logs for further details.");
            }
        }
        public delegate void ImportDel(PokemonRenderData? data);
        public event ImportDel? OnImport;
        
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        private async Task Export()
        {
            List<PokemonRenderData> data = MainWindow.SelectedPokemonRenderData.Where(prd => prd is not null).Cast<PokemonRenderData>().ToList();
            if (data.Count == 1)
            {
                PokemonRenderData renderData = data.First();

                List<string> extensions = new() {
                    "*.json"
                };
                List<FilePickerFileType> filters = new()
                {
                    new FilePickerFileType("PKX-IconGen Json") { Patterns = extensions }
                };
                IStorageFile? file = await FileDialogHelper.SaveFile("Export Render Data", filters, initialFileName: renderData.Output + ".json", defaultExtension: "json");
                if (file != null)
                {
                    await using Stream fileStream = await file.OpenWriteAsync();
                    await JsonIO.ExportAsync(renderData, fileStream);
                }
            }
            else if (data.Count > 1)
            {
                IStorageFolder? directory = await FileDialogHelper.GetFolder("Export Render Data");
                if (directory != null)
                {
                    foreach (PokemonRenderData prd in data)
                    {
                        await JsonIO.ExportAsync(prd, Path.Combine(directory.Path.AbsolutePath, prd.Output + ".json"));
                    }
                }
            }
        }
        
        public ReactiveCommand<Unit, Unit> ExportBlenderCommand { get; }
        private async Task ExportBlender()
        {
            await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "Exporting to a Blender scene is not yet supported.");
            //throw new NotImplementedException();
        }

        public ReactiveCommand<Unit,Unit> OpenSettingsCommand { get; }
        public async Task OpenSettings()
        {
            Settings settings = await DoDBQueryAsync(db => db.GetSettingsAsync());
            SettingsWindowViewModel dialogVm = new(settings);
            await DialogHelper.ShowWindowDialog<SettingsWindowViewModel>(dialogVm);
        }

        [UsedImplicitly]
        public async void CleanTempFolders()
        {
            bool delete = await DialogHelper.ShowDialog(DialogType.Warning, DialogButtons.YesNo, "Are you sure you want to clean the temporary folders? This includes the Logs folder and the downloaded HD textures. The HD textures will remain installed.\nThis operation is irreversible.", 220);
            if (delete)
            {
                await Core.Utils.CleanTempFolders();
            }
        }
        
        [UsedImplicitly]
        public async void DownloadTextures()
        {
            using TextureDownloadWindowViewModel vm = new(MainWindow.AssetsPath);
            await DialogHelper.ShowWindowDialog<TextureDownloadWindowViewModel, object>(vm);
        }

        [UsedImplicitly]
        public void GitHub()
        {
            Utils.OpenUrl("https://github.com/mikeyX101/PKX-IconGen");
        }
        
        [UsedImplicitly]
        public void ImporterAddon()
        {
            Utils.OpenUrl("https://github.com/StarsMmd/Blender-Addon-Gamecube-Models");
        }

        [UsedImplicitly]
        public static async void About()
        {
            Assembly coreAssembly = Assembly.Load("PKX-IconGen.Core");
            Assembly uiAssembly = Assembly.GetExecutingAssembly();
            Assembly avaloniaAssembly = Assembly.Load("Avalonia");

            await DialogHelper.ShowDialog("/Assets/gen-icon.png",
                DialogButtons.Ok,
                $"""
                 PKX-IconGen by mikeyX
                 Core: {coreAssembly.GetName().Version?.ToString() ?? "Unknown Version"}
                 UI: {uiAssembly.GetName().Version?.ToString() ?? "Unknown Version"}
                 Blender Addon: {Versions.AddonVersion}
                 Importer: Commit {Versions.ImporterCommit[..7]} on the {Versions.ImporterDate:yyyy-MM-dd}

                 Powered by Avalonia {avaloniaAssembly.GetName().Version?.ToString() ?? "Unknown Version"} 
                 """,
                height: 275, title: "About");
        }

        [UsedImplicitly]
        public static void Quit()
        {
            Utils.GetApplicationLifetime().Shutdown(0);
        }
    }
}
