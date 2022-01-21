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
using PKXIconGen.AvaloniaUI.Interfaces;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private IExportableDataProvider<PokemonRenderData> ExportProvider { get; }

        public MenuViewModel(IExportableDataProvider<PokemonRenderData> exportProvider)
        {
            OnImport += data => { };

            ExportProvider = exportProvider;

            // Reactive
            IObservable<bool> exportEnabled = this.WhenAnyValue(
                vm => vm.ExportProvider.ExportData,
                data => data.Any());

            ExportCommand = ReactiveCommand.CreateFromTask(Export, exportEnabled);
            ExportBlenderCommand = ReactiveCommand.CreateFromTask(ExportBlender, exportEnabled);
        }

        public async void Import()
        {
            List<FileDialogFilter>? filters = null;
            if (IsWindows)
            {
                filters = new();
                List<string> extensions = new() {
                    "json"
                };
                filters.Add(new FileDialogFilter { Extensions = extensions, Name = "PKX-IconGen Json" });
            }

            string[]? paths = await FileDialogHelper.GetFiles("Select Json files to import...", filters);
            if (paths != null)
            {
                try
                {
                    await foreach (PokemonRenderData? data in JsonIO.ImportAsyncEnumerable<PokemonRenderData>(paths))
                    {
                        OnImport(data);
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Core.CoreManager.Logger.Error(ex, "An exception occured while importing data. Probably invalid Json for this case.");
                    await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "An error occured while importing. The given Json is invalid.\nClose the application and see the logs for further details.");
                }
                catch (Exception ex) 
                {
                    Core.CoreManager.Logger.Error(ex, "An unexpected exception occured while importing data.");
                    await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "An unexpected error occured while importing. \nClose the application and see the logs for further details.");
                }
            }
        }
        public delegate void ImportDel(PokemonRenderData? data);
        public event ImportDel OnImport;

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public async Task Export()
        {
            IList<PokemonRenderData>? data = ExportProvider.ExportData?.Where(prd => prd != null).ToList();
            if (data != null && data.Count == 1)
            {
                PokemonRenderData renderData = data.First();

                List<string> extensions = new() {
                    "json"
                };
                List<FileDialogFilter>? filters = new(new List<FileDialogFilter> { new FileDialogFilter { Extensions = extensions, Name = "PKX-IconGen Json" } });
                string? filePath = await FileDialogHelper.SaveFile("Export Render Data", filters, initialFileName: renderData.Name.ToLower() + ".json");
                if (filePath != null)
                {
                    await JsonIO.ExportAsync(renderData, filePath);
                }
            }
            else if (data != null && data.Count > 1)
            {
                string? directory = await FileDialogHelper.GetFolder("Export Render Data");
                if (directory != null)
                {
                    foreach (PokemonRenderData prd in data)
                    {
                        await JsonIO.ExportAsync(prd, Path.Combine(directory, prd.Name + ".json"));
                    }
                }
            }
        }
        public ReactiveCommand<Unit, Unit> ExportBlenderCommand { get; }
        public async Task ExportBlender()
        {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        public void SysDolphinAddon()
        {
            string url = "https://github.com/StarsMmd/Blender-Addon-Gamecube-Models";

            // For .NETCore 3 and more: https://stackoverflow.com/a/43232486
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (IsWindows)
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (IsMacOS)
            {
                Process.Start("xdg-open", url);
            }
            else if (IsLinux)
            {
                Process.Start("open", url);
            }
        }

        public async void About()
        {
            Assembly coreAssembly = Assembly.Load("PKX-IconGen.Core");
            Assembly uiAssembly = Assembly.GetExecutingAssembly();
            Assembly avaloniaAssembly = Assembly.Load("Avalonia");

            await DialogHelper.ShowDialog("/Assets/gen-icon-x512.png",
                DialogButtons.Ok,
@$"PKX-IconGen by mikeyX
Core: {coreAssembly.GetName().Version?.ToString() ?? "Unknown"} 
UI: {uiAssembly.GetName().Version?.ToString() ?? "Unknown"}

Powered by Avalonia {avaloniaAssembly.GetName().Version?.ToString() ?? "Unknown"} ",
                "About");
        }

        public void Quit()
        {
            Utils.GetApplicationLifetime().Shutdown(0);
        }
    }
}
