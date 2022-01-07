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
using Avalonia.Controls.Selection;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : WindowViewModelBase, IBlenderRunnerInfo
    {
        #region ViewModels
        public MenuViewModel MenuVM { get; set; }
        public LogViewModel LogVM { get; set; }
        #endregion

        #region Blender Path
        public string Path => BlenderPath;
        private string blenderPath;
        public string BlenderPath {
            get => blenderPath;
            set {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.BlenderPath, value));
                this.RaiseAndSetIfChanged(ref blenderPath, value);
                VerifyBlenderExecutable();
            }
        }
        private bool isBlenderValid;
        public bool IsBlenderValid {
            get => isBlenderValid;
            set {
                this.RaiseAndSetIfChanged(ref isBlenderValid, value);
            }
        }
        private bool isBlenderVersionValid;
        public bool IsBlenderVersionValid {
            get => isBlenderVersionValid;
            set {
                this.RaiseAndSetIfChanged(ref isBlenderVersionValid, value);
            }
        }
        private string blenderWarningText = "";
        public string BlenderWarningText
        {
            get => blenderWarningText;
            set {
                this.RaiseAndSetIfChanged(ref blenderWarningText, value);
            }
        }
        private string blenderWarningClass = "";
        private string BlenderWarningClass {
            get => blenderWarningClass;
            set {
                blenderWarningClass = value;
                this.RaisePropertyChanged(nameof(BlenderPathWarning));
                this.RaisePropertyChanged(nameof(BlenderPathError));
            }
        }
        public bool BlenderPathWarning => BlenderWarningClass == "Warning";
        public bool BlenderPathError => BlenderWarningClass == "Error";
        #endregion

        #region Blender Optional Arguments
        public string OptionalArguments => BlenderOptionalArguments;
        private string blenderOptionalArguments;
        public string BlenderOptionalArguments {
            get => blenderOptionalArguments;
            set {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.BlenderOptionalArguments, value));
                this.RaiseAndSetIfChanged(ref blenderOptionalArguments, value);
            }
        }
        #endregion

        #region Output Path
        private string outputPath;
        public string OutputPath {
            get => outputPath;
            set {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.OutputPath, value));
                this.RaiseAndSetIfChanged(ref outputPath, value);
            }
        }
        #endregion

        #region Icon Style
        public IconStyle[] IconStyleItems { get; private set; }
        private IconStyle selectedIconStyle;
        public IconStyle SelectedIconStyle {
            get => selectedIconStyle; 
            set
            {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.CurrentGame, value.Game));
                this.RaiseAndSetIfChanged(ref selectedIconStyle, value);
            }
        }
        #endregion

        #region Render Scale
        public RenderScale[] RenderScaleItems { get; private set; }
        private RenderScale selectedRenderScale;
        public RenderScale SelectedRenderScale {
            get => selectedRenderScale; 
            set
            {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.RenderScale, value));
                this.RaiseAndSetIfChanged(ref selectedRenderScale, value);
            }
        }
        #endregion

        #region Pokemon Render Data
        private IList<PokemonRenderData> AllPokemonRenderDataItems { get; }
        public IList<PokemonRenderData> PokemonRenderDataItems { get; }
        public SelectionModel<PokemonRenderData> PokemonRenderDataSelection { get; }
        private bool enableDeleteButton;
        public bool EnableDeleteButton
        {
            get => enableDeleteButton;
            set
            {
                this.RaiseAndSetIfChanged(ref enableDeleteButton, value);
            }
        }

        private bool builtInsHidden;
        public bool BuiltInsHidden
        {
            get => builtInsHidden;
            set
            {
                this.RaiseAndSetIfChanged(ref builtInsHidden, value);
                PokemonRenderDataItems.Clear();
                if (value)
                {
                    foreach (PokemonRenderData renderData in AllPokemonRenderDataItems.Where(prd => !prd.BuiltIn))
                    {
                        PokemonRenderDataItems.Add(renderData);
                    }
                }
                else
                {
                    foreach (PokemonRenderData renderData in AllPokemonRenderDataItems)
                    {
                        PokemonRenderDataItems.Add(renderData);
                    }
                }
            }
        }
        #endregion

        #region Assets Path
        private string assetsPath;
        public string AssetsPath
        {
            get => assetsPath;
            set
            {
                DoDBQuery(db => db.SaveSettingsProperty(s => s.AssetsPath, value));
                this.RaiseAndSetIfChanged(ref assetsPath, value);
            }
        }
        #endregion

        #region Progress

        private bool currentlyRendering;
        public bool CurrentlyRendering
        {
            get => currentlyRendering;
            set=> this.RaiseAndSetIfChanged(ref currentlyRendering, value);
        }
        private uint nbOfRenders;
        private uint NbOfRenders {
            get => nbOfRenders;
            set {
                nbOfRenders = value;
                this.RaisePropertyChanged(nameof(DisplayPokemonRendered));
                this.RaisePropertyChanged(nameof(PercentPokemonRendered));
            }
        }
        private uint nbOfPokemonRendered;
        private uint NbOfPokemonRendered {
            get => nbOfPokemonRendered;
            set {
                nbOfPokemonRendered = value;
                this.RaisePropertyChanged(nameof(DisplayPokemonRendered));
                this.RaisePropertyChanged(nameof(PercentPokemonRendered));
            }
        }
        public string DisplayPokemonRendered
        {
            get
            {
                return $"{NbOfPokemonRendered}/{NbOfRenders}";
            }
        }
        public string PercentPokemonRendered
        {
            get
            {
                if (NbOfRenders == 0 || NbOfPokemonRendered == 0)
                {
                    return "0%";
                }
                return Math.Round(NbOfRenders / NbOfPokemonRendered * 100d) + "%";
            }
        }
        #endregion

        public MainWindowViewModel(Settings settings, IList<PokemonRenderData> renderData) : base()
        {
            // VMs
            MenuVM = new();
            LogVM = new();

            // Fields
            blenderPath = settings.BlenderPath;
            VerifyBlenderExecutable();

            blenderOptionalArguments = settings.BlenderOptionalArguments;

            outputPath = settings.OutputPath;

            IconStyleItems = IconStyle.GetIconStyles();
            selectedIconStyle = IconStyleItems.Where(i => i.Game == settings.CurrentGame).First();

            RenderScaleItems = Enum.GetValues<RenderScale>();
            SelectedRenderScale = settings.RenderScale;

            AllPokemonRenderDataItems = renderData;
            PokemonRenderDataItems = new ObservableCollection<PokemonRenderData>(renderData);
            PokemonRenderDataSelection = new()
            {
                SingleSelect = false,
                Source = PokemonRenderDataItems
            };
            PokemonRenderDataSelection.SelectionChanged += PokemonRenderDataSelection_SelectionChanged;
            enableDeleteButton = false;
            builtInsHidden = false;

            assetsPath = settings.AssetsPath;

            currentlyRendering = false;

            // Reactive
            IObservable<bool> renderEnabled = this.WhenAnyValue(
                vm => vm.IsBlenderValid, vm => vm.OutputPath, vm => vm.SelectedIconStyle, 
                (blenderValid, outputPath, iconStyle) => blenderValid && !string.IsNullOrWhiteSpace(outputPath) && iconStyle.Game != Game.Undefined
            );

            RenderCommand = ReactiveCommand.Create(Render, renderEnabled);
        }

        #region Paths Logic
        public void VerifyBlenderExecutable()
        {
            BlenderVersionChecker versionChecker = new();
            BlenderCheckResult? blenderCheckResult = versionChecker.CheckExecutable(BlenderPath);
            if (blenderCheckResult != null)
            {
                IsBlenderValid = blenderCheckResult.IsBlender;
                IsBlenderVersionValid = blenderCheckResult.IsValidVersion;
                if (IsBlenderValid && !IsBlenderVersionValid)
                {
                    BlenderWarningClass = "Warning";
                    BlenderWarningText = $"This Blender version might not be compatible with the addon. Detected version: {blenderCheckResult.Version}";
                    return;
                }
                else if (IsBlenderValid && IsBlenderVersionValid)
                {
                    BlenderWarningClass = "";
                    BlenderWarningText = "";
                    return;
                }
            }
            IsBlenderValid = IsBlenderVersionValid = false;

            BlenderWarningClass = "Error";
            BlenderWarningText = "Please specify a Blender executable.";
        }

        public async void BrowseBlenderPath()
        {
            List<FileDialogFilter>? filters = null;
            if (IsWindows)
            {
                filters = new();
                List<string> extensions = new() {
                    "exe"
                };
                filters.Add(new FileDialogFilter { Extensions = extensions, Name = "Blender" });
            }

            string? path = await FileDialogHelper.GetFile("Select a Blender executable...", filters);
            if (path != null)
            {
                BlenderPath = path;
            }
        }

        public async void BrowseOutputPath()
        {
            string? folder = await FileDialogHelper.GetFolder("Select an output path...");
            if (folder != null)
            {
                OutputPath = folder;
            }
        }

        public async void BrowseAssetsPath()
        {
            string? folder = await FileDialogHelper.GetFolder("Select the assets path...");
            if (folder != null)
            {
                AssetsPath = folder;
            }
        }
        #endregion

        #region Pokemon DataGrid
        private void PokemonRenderDataSelection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<PokemonRenderData> e)
        {
            EnableDeleteButton = PokemonRenderDataSelection.SelectedItems.Count > 0 && PokemonRenderDataSelection.SelectedItems.All(prd => !prd.BuiltIn);
        }

        public async Task NewRenderData()
        {
            PokemonRenderData renderData = await DialogHelper.ShowWindowDialog<PokemonRenderDataWindowViewModel, PokemonRenderData>(new PokemonRenderDataWindowViewModel(this));
        }

        public async void DeleteSelectedRenderData()
        {
            if (await DialogHelper.ShowDialog(Models.Dialog.DialogType.Warning, Models.Dialog.DialogButtons.YesNo, "Are you sure you want to delete these Pokemon render data?\nThis operation is irreversible."))
            {
                if (DoDBQuery(db => db.DeletePokemonRenderData(PokemonRenderDataSelection.SelectedItems)) > 0)
                {
                    List<PokemonRenderData> temp = PokemonRenderDataItems.Where(prd => !PokemonRenderDataSelection.SelectedItems.Contains(prd)).ToList();
                    PokemonRenderDataItems.Clear();
                    foreach (PokemonRenderData renderData in temp)
                    {
                        PokemonRenderDataItems.Add(renderData);
                    }
                    PokemonRenderDataSelection.Clear();
                }
            }

        }

        public void SelectAllRenderData() => PokemonRenderDataSelection.SelectAll();
        public void DeselectAllRenderData() => PokemonRenderDataSelection.Clear();

        #endregion

        #region Render
        private CancellationTokenSource? renderCancelTokenSource = null;
        public ReactiveCommand<Unit, Unit> RenderCommand { get; }
        public async void Render()
        {
            DisposeCancelToken();
            renderCancelTokenSource = new();

            BlenderRunner runner = new(this, new string[] { "--background" });
            runner.OnOutput += LogVM.Write;
            runner.OnFinish += EndRender;

            CurrentlyRendering = true;
            await runner.RunRenderAsync(renderCancelTokenSource.Token);
        }
        public void EndRender()
        {
            DisposeCancelToken();
            CurrentlyRendering = false;
        }
        private void DisposeCancelToken()
        {
            // Make sure tokens are canceled
            if (renderCancelTokenSource != null)
            {
                renderCancelTokenSource.Cancel();
                renderCancelTokenSource.Dispose();
                renderCancelTokenSource = null;
            }
        }
        #endregion
    }
}
