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
using DynamicData;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing;
using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : WindowViewModelBase, IBlenderRunnerInfo
    {
        #region ViewModels
        public MenuViewModel MenuVM { get; init; }
        public LogViewModel LogVM { get; init; }
        #endregion

        #region Log Blender
        private bool logBlender;
        public bool LogBlender
        {
            get => logBlender;
            set { 
                DoDBQuery(db => db.SaveSettingsProperty(s => s.LogBlender, value));
                this.RaiseAndSetIfChanged(ref logBlender, value); 
            }
        }
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
        private bool IsBlenderValid {
            get => isBlenderValid;
            set => this.RaiseAndSetIfChanged(ref isBlenderValid, value);
        }
        private bool isBlenderVersionValid;
        public bool IsBlenderVersionValid {
            get => isBlenderVersionValid;
            private set => this.RaiseAndSetIfChanged(ref isBlenderVersionValid, value);
        }
        private string blenderWarningText = "";
        public string BlenderWarningText
        {
            get => blenderWarningText;
            private set => this.RaiseAndSetIfChanged(ref blenderWarningText, value);
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
        private SourceList<PokemonRenderData> PokemonRenderDataItemsSource { get; init; }
        private readonly ReadOnlyObservableCollection<PokemonRenderData> pokemonRenderDataItems;
        public ReadOnlyObservableCollection<PokemonRenderData> PokemonRenderDataItems => pokemonRenderDataItems;

        public SelectionModel<PokemonRenderData> PokemonRenderDataSelection { get; init; }
        public IReadOnlyList<PokemonRenderData> SelectedPokemonRenderData => PokemonRenderDataSelection.SelectedItems;

        private bool enableDeleteButton;
        public bool EnableDeleteButton
        {
            get => enableDeleteButton;
            set => this.RaiseAndSetIfChanged(ref enableDeleteButton, value);
        }

        private bool builtInsHidden;
        public bool BuiltInsHidden
        {
            get => builtInsHidden;
            set
            {
                this.RaiseAndSetIfChanged(ref builtInsHidden, value);

                List<PokemonRenderData> builtIns = DoDBQuery(db => db.GetPokemonRenderDataBuiltIns());
                if (value)
                {
                    PokemonRenderDataItemsSource.RemoveMany(builtIns);
                }
                else
                {
                    PokemonRenderDataItemsSource.AddRange(builtIns);
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
            set => this.RaiseAndSetIfChanged(ref currentlyRendering, value);
        }
        private int nbOfRenders;
        public int NbOfRenders
        {
            get => nbOfRenders;
            set
            {
                this.RaiseAndSetIfChanged(ref nbOfRenders, value);
                this.RaisePropertyChanged(nameof(DisplayPokemonRendered));
                this.RaisePropertyChanged(nameof(PercentPokemonRendered));
            }
        }
        private uint nbOfPokemonRendered;
        public uint NbOfPokemonRendered {
            get => nbOfPokemonRendered;
            set {
                this.RaiseAndSetIfChanged(ref nbOfPokemonRendered, value);
                this.RaisePropertyChanged(nameof(DisplayPokemonRendered));
                this.RaisePropertyChanged(nameof(PercentPokemonRendered));
            }
        }
        public string DisplayPokemonRendered => $"{NbOfPokemonRendered}/{NbOfRenders}";

        public string PercentPokemonRendered
        {
            get
            {
                if (NbOfRenders == 0 || NbOfPokemonRendered == 0)
                {
                    return "0%";
                }
                return Math.Round(100d * NbOfPokemonRendered / NbOfRenders) + "%";
            }
        }
        #endregion

        public MainWindowViewModel(Settings settings, IEnumerable<PokemonRenderData> renderData) : base()
        {
            // Reactive collections
            PokemonRenderDataItemsSource = new SourceList<PokemonRenderData>();
            PokemonRenderDataItemsSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out pokemonRenderDataItems)
                .Subscribe();
            PokemonRenderDataItemsSource.AddRange(renderData);
            PokemonRenderDataSelection = new SelectionModel<PokemonRenderData>()
            {
                SingleSelect = false,
                Source = PokemonRenderDataItems
            };
            PokemonRenderDataSelection.SelectionChanged += PokemonRenderDataSelection_SelectionChanged;
            /*SelectedPokemonRenderDataSource = new SourceList<PokemonRenderData>();
            SelectedPokemonRenderDataSource.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out selectedPokemonRenderData)
                .Subscribe(_ => { }, () => NbOfRenders = selectedPokemonRenderData.Count);
            */
            // VMs
            MenuVM = new MenuViewModel(this);
            MenuVM.OnImport += AddRenderData;
            LogVM = new LogViewModel();

            // Fields
            logBlender = settings.LogBlender;

            blenderPath = settings.BlenderPath;
            VerifyBlenderExecutable();

            blenderOptionalArguments = settings.BlenderOptionalArguments;

            outputPath = settings.OutputPath;

            IconStyleItems = IconStyle.GetIconStyles();
            selectedIconStyle = IconStyleItems.First(i => i.Game == settings.CurrentGame);

            RenderScaleItems = Enum.GetValues<RenderScale>();
            SelectedRenderScale = settings.RenderScale;

            enableDeleteButton = false;
            builtInsHidden = false;

            assetsPath = settings.AssetsPath;


            currentlyRendering = false;

            // Reactive
            IObservable<bool> renderEnabled = this.WhenAnyValue(
                vm => vm.IsBlenderValid, vm => vm.OutputPath, vm => vm.SelectedIconStyle, vm => vm.NbOfRenders,
                (blenderValid, op, iconStyle, renderNum) => blenderValid && !string.IsNullOrWhiteSpace(op) && Directory.Exists(op) && iconStyle.Game != Game.Undefined && renderNum > 0
            );
            RenderCommand = ReactiveCommand.Create(Render, renderEnabled);

            IObservable<bool> renderDataOperationsEnabled = this.WhenAnyValue(
                vm => vm.IsBlenderValid, 
                blenderValid => (bool)blenderValid);
            NewRenderDataCommand = ReactiveCommand.CreateFromTask(NewRenderData, renderDataOperationsEnabled);
            EditRenderDataCommand = ReactiveCommand.CreateFromTask<PokemonRenderData>(EditRenderData, renderDataOperationsEnabled);
        }

        private async void AddRenderData(PokemonRenderData? data)
        {
            if (data is not null)
            {
                int changed = await DoDBQueryAsync(db => db.AddPokemonRenderDataAsync(data));
                if (changed > 0)
                {
                    PokemonRenderDataItemsSource.Add(data);
                }
            }
        }

        #region Paths Logic
        private void VerifyBlenderExecutable()
        {
            BlenderCheckResult? blenderCheckResult = BlenderVersionChecker.CheckExecutable(BlenderPath);
            if (blenderCheckResult != null)
            {
                IsBlenderValid = blenderCheckResult.Value.IsBlender;
                IsBlenderVersionValid = blenderCheckResult.Value.IsValidVersion;
                if (IsBlenderValid && !IsBlenderVersionValid)
                {
                    BlenderWarningClass = "Warning";
                    BlenderWarningText = $"This Blender version might not be compatible with the addon and could crash while importing. Detected version: {blenderCheckResult.Value.Version.ToString(CultureInfo.InvariantCulture)}";
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

        #region Pokemon List
        private void PokemonRenderDataSelection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<PokemonRenderData> e)
        {
            EnableDeleteButton = SelectedPokemonRenderData.Count > 0 && SelectedPokemonRenderData.All(prd => !prd.BuiltIn);
            NbOfRenders = SelectedPokemonRenderData.Count;
            /*if (e.SelectedItems.Count > 0)
            {
                SelectedPokemonRenderDataSource.AddRange(e.SelectedItems);
            }
            if (e.DeselectedItems.Count > 0)
            {
                SelectedPokemonRenderDataSource.RemoveMany(e.DeselectedItems);
            }*/
        }

        [UsedImplicitly]
        public ReactiveCommand<Unit,Unit> NewRenderDataCommand { get; }
        private async Task NewRenderData()
        {
            PokemonRenderData? renderData = await DialogHelper.ShowWindowDialog<PokemonRenderDataWindowViewModel, PokemonRenderData?>(new PokemonRenderDataWindowViewModel("Add a new Pokemon...", this, null));
            AddRenderData(renderData);
        }

        [UsedImplicitly]
        public ReactiveCommand<PokemonRenderData,Unit> EditRenderDataCommand { get; }
        private async Task EditRenderData(PokemonRenderData prd)
        {
            PokemonRenderDataWindowViewModel dialogVM = new("Edit", this, prd);
            PokemonRenderData? newRenderData = await DialogHelper.ShowWindowDialog<PokemonRenderDataWindowViewModel, PokemonRenderData?>(dialogVM);
            if (newRenderData is not null && await DoDBQueryAsync(db => db.EditPokemonRenderDataAsync(prd.ID, newRenderData)) > 0)
            {
                PokemonRenderDataItemsSource.Replace(prd, newRenderData);
            }
            dialogVM.DisposeCancelToken();
        }
        
        [UsedImplicitly]
        public async void DeleteSelectedRenderData()
        {
            if (await DialogHelper.ShowDialog(Models.Dialog.DialogType.Warning, Models.Dialog.DialogButtons.YesNo, "Are you sure you want to delete these Pokemon render data?\nThis operation is irreversible."))
            {
                if (await DoDBQueryAsync(db => db.DeletePokemonRenderDataAsync(PokemonRenderDataSelection.SelectedItems)) > 0)
                {
                    PokemonRenderDataItemsSource.RemoveMany(PokemonRenderDataSelection.SelectedItems);
                }
            }
        }

        [UsedImplicitly]
        public void SelectAllRenderData() => PokemonRenderDataSelection.SelectAll();
        public void DeselectAllRenderData() => PokemonRenderDataSelection.Clear();

        #endregion

        #region Render
        private CancellationTokenSource? renderCancelTokenSource;
        [UsedImplicitly]
        public ReactiveCommand<Unit, Unit> RenderCommand { get; }
        private async void Render()
        {
            DisposeCancelToken();
            renderCancelTokenSource = new CancellationTokenSource();
            NbOfPokemonRendered = 0;
            CurrentlyRendering = true;

            foreach (RenderJob job in SelectedPokemonRenderData.Select(prd => new RenderJob(prd, SelectedRenderScale, SelectedIconStyle.Game, OutputPath)))
            {
                if (renderCancelTokenSource.IsCancellationRequested)
                {
                    break;
                }

                await job.RenderAsync(this, renderCancelTokenSource.Token, LogVM.WriteLine);
                NbOfPokemonRendered++;
            }

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
