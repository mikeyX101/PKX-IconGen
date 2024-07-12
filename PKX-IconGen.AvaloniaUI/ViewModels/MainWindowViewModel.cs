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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using JetBrains.Annotations;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels;

public sealed partial class MainWindowViewModel : WindowViewModelBase, IDisposable
{
    #region ViewModels
    public MenuViewModel MenuVM { get; init; }
    public LogViewModel LogVM { get; init; }
    #endregion

    #region Blender Path
    private string blenderPath = "";
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
    private string blenderOptionalArguments = "";
    public string BlenderOptionalArguments {
        get => blenderOptionalArguments;
        set {
            DoDBQuery(db => db.SaveSettingsProperty(s => s.BlenderOptionalArguments, value));
            this.RaiseAndSetIfChanged(ref blenderOptionalArguments, value);
        }
    }
    #endregion

    #region Output Path
    private string outputPath = "";
    public string OutputPath {
        get => outputPath;
        set {
            DoDBQuery(db => db.SaveSettingsProperty(s => s.OutputPath, value));
            this.RaiseAndSetIfChanged(ref outputPath, value);
        }
    }
    #endregion

    #region Icon Style

    public IconStyle[] IconStyleItems { get; } = IconStyle.GetIconStyles();
    private IconStyle selectedIconStyle = Game.Undefined.GetIconStyle();
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
    public RenderScale[] RenderScaleItems { get; } = Enum.GetValues<RenderScale>();
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
    private SourceCache<PokemonRenderData, uint> PokemonRenderDataItemsSource { get; }
    private readonly ReadOnlyObservableCollection<PokemonRenderData> pokemonRenderDataItems;
    public ReadOnlyObservableCollection<PokemonRenderData> PokemonRenderDataItems => pokemonRenderDataItems;

    public SelectionModel<PokemonRenderData> PokemonRenderDataSelection { get; init; }
    public IReadOnlyList<PokemonRenderData?> SelectedPokemonRenderData => PokemonRenderDataSelection.SelectedItems;

    private bool enableDeleteButton;
    public bool EnableDeleteButton
    {
        get => enableDeleteButton;
        set => this.RaiseAndSetIfChanged(ref enableDeleteButton, value);
    }

    #endregion

    #region Assets Path
    [GeneratedRegex(@"(/|\\)+$")]
    private static partial Regex AssetPathRegex();
    
    private string assetsPath = "";
    public string AssetsPath
    {
        get => assetsPath;
        set
        {
            value = AssetPathRegex().Replace(value, "");
                
            DoDBQuery(db => db.SaveSettingsProperty(s => s.AssetsPath, value));
            this.RaiseAndSetIfChanged(ref assetsPath, value);
            this.RaisePropertyChanged(nameof(AssetsPathIsValid));
            this.RaisePropertyChanged(nameof(DefinedAssetsPathIsValid));
        }
    }
        
    public bool AssetsPathIsValid => string.IsNullOrWhiteSpace(AssetsPath) || Directory.Exists(AssetsPath);
    public bool DefinedAssetsPathIsValid => !string.IsNullOrWhiteSpace(AssetsPath) && Directory.Exists(AssetsPath);
    #endregion

    #region Progress

    private bool initialLoadingFinished;
    public bool InitialLoadingFinished
    {
        get => initialLoadingFinished;
        private set => this.RaiseAndSetIfChanged(ref initialLoadingFinished, value);
    }
        
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
        
    private bool devTools;
    public bool DevTools
    {
        get => devTools;
        set => this.RaiseAndSetIfChanged(ref devTools, value);
    }

    public MainWindowViewModel(Task databaseLoadingTask)
    {
        // Reactive collections
        PokemonRenderDataItemsSource = new SourceCache<PokemonRenderData, uint>(prd => prd.Id);
        PokemonRenderDataItemsSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .AutoRefresh(prd => prd.Name)
            .AutoRefresh(prd => prd.FaceRender)
            .BindToObservableList(out IObservableList<PokemonRenderData> observableList)
            .Subscribe();
            
        observableList.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<PokemonRenderData>.Ascending(prd => prd.Name))
            .Bind(out pokemonRenderDataItems)
            .Subscribe();
            
        PokemonRenderDataSelection = new SelectionModel<PokemonRenderData>
        {
            SingleSelect = false
        };
        PokemonRenderDataSelection.SelectionChanged += PokemonRenderDataSelection_SelectionChanged;
            
        // VMs
        MenuVM = new MenuViewModel(this);
        MenuVM.OnImport += AddRenderData;
        LogVM = new LogViewModel();

        // Reactive
        IObservable<bool> renderEnabled = this.WhenAnyValue(
            vm => vm.IsBlenderValid,
            vm => vm.OutputPath, 
            vm => vm.SelectedIconStyle, 
            vm => vm.NbOfRenders,
            vm => vm.AssetsPath,
            vm => vm.AssetsPathIsValid,
            vm => vm.SelectedPokemonRenderData,
            (blenderValid, output, iconStyle, renderNum, assets, assetsPathValid, selectedPRDs) => 
                blenderValid &&
                !string.IsNullOrWhiteSpace(output) && Directory.Exists(output) && 
                iconStyle.Game != Game.Undefined && 
                renderNum > 0 && 
                assetsPathValid &&
                (
                    !string.IsNullOrWhiteSpace(assets) || // Good if we have an assets path
                    selectedPRDs.All(prd =>  // otherwise check selected PRDs for model paths containing {{AssetsPath}}
                        (!prd?.Model.Contains("{{AssetsPath}}") ?? false) && // Normal model doesn't contain {{AssetsPath}}
                        (
                            string.IsNullOrWhiteSpace(prd!.Shiny.Model) || // No Shiny model or
                            !prd.Shiny.Model.Contains("{{AssetsPath}}") // Shiny model doesn't contain {{AssetsPath}}
                        )
                    )
                )
        );
        RenderCommand = ReactiveCommand.CreateFromTask<RenderTarget>(Render, renderEnabled);

        IObservable<bool> renderDataOperationsEnabled = this.WhenAnyValue(
            vm => vm.IsBlenderValid, 
            vm => vm.AssetsPathIsValid,
            (blenderValid, assetsPathValid) => 
                blenderValid && 
                assetsPathValid
        );
        NewRenderDataCommand = ReactiveCommand.CreateFromTask(NewRenderData, renderDataOperationsEnabled);
        EditRenderDataCommand = ReactiveCommand.CreateFromTask<PokemonRenderData>(EditRenderData, renderDataOperationsEnabled);

        SetInitialState(databaseLoadingTask);
    }

    private async void SetInitialState(Task databaseLoadingTask)
    {
        await databaseLoadingTask;
        Database db = Database.Instance;
            
        Settings settings = await db.GetSettingsAsync();
        IEnumerable<PokemonRenderData> renderData = await db.GetPokemonRenderDataAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            PokemonRenderDataItemsSource.AddOrUpdate(renderData);
            
            // Fields
            BlenderPath = settings.BlenderPath;
            BlenderOptionalArguments = settings.BlenderOptionalArguments;
            OutputPath = settings.OutputPath;
            SelectedIconStyle = IconStyleItems.First(i => i.Game == settings.CurrentGame);
            SelectedRenderScale = settings.RenderScale;
            EnableDeleteButton = false;
            AssetsPath = settings.AssetsPath;
            DevTools = settings.DevTools;
                
            InitialLoadingFinished = true;
        }, DispatcherPriority.Background);
    }

    private async void AddRenderData(PokemonRenderData? data)
    {
        if (data is null) 
            return;
        
        int id = await DoDBQueryAsync(db => db.AddPokemonRenderDataAsync(data));
        if (id > 0)
        {
            PokemonRenderDataItemsSource.AddOrUpdate(data);
        }
    }

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
            if (IsBlenderValid && IsBlenderVersionValid)
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
        
    #region Pokemon List
    private void PokemonRenderDataSelection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<PokemonRenderData> e)
    {
        EnableDeleteButton = SelectedPokemonRenderData.Count > 0;
        NbOfRenders = SelectedPokemonRenderData.Count;
        NbOfPokemonRendered = 0;
    }
        
    public ReactiveCommand<Unit,Unit> NewRenderDataCommand { get; }
    private async Task NewRenderData()
    {
        PokemonRenderData newData = new();
        Settings settings = await DoDBQueryAsync(db => db.GetSettingsAsync());
        using PokemonRenderDataWindowViewModel dialogVm = new("Add a new Pokemon...", settings, newData);
        bool toSave = await DialogHelper.ShowWindowDialog<PokemonRenderDataWindowViewModel, bool>(dialogVm);
        if (toSave)
        {
            AddRenderData(newData);
        }
    }
        
    [UsedImplicitly]
    public ReactiveCommand<PokemonRenderData,Unit> EditRenderDataCommand { get; }
    private async Task EditRenderData(PokemonRenderData prd)
    {
        PokemonRenderData copy = (PokemonRenderData)prd.Clone();
        Settings settings = await DoDBQueryAsync(db => db.GetSettingsAsync());
        using PokemonRenderDataWindowViewModel dialogVm = new("Edit", settings, copy);
        bool toSave = await DialogHelper.ShowWindowDialog<PokemonRenderDataWindowViewModel, bool>(dialogVm);
        if (toSave && await DoDBQueryAsync(db => db.UpdatePokemonRenderDataAsync(copy)) > 0)
        {
            PokemonRenderDataItemsSource.AddOrUpdate(copy);
            PokemonRenderDataItemsSource.Refresh(copy);
        }
    }
        
    [UsedImplicitly]
    public async void DeleteSelectedRenderData()
    {
        if (!await DialogHelper.ShowDialog(Models.Dialog.DialogType.Warning, Models.Dialog.DialogButtons.YesNo, "Are you sure you want to delete these Pokemon?\nThis operation is irreversible.")) 
            return;
        
        IEnumerable<PokemonRenderData> selectedPRDs = PokemonRenderDataSelection.SelectedItems.Where(prd => prd is not null).Cast<PokemonRenderData>();
        if (await DoDBQueryAsync(db => db.DeletePokemonRenderDataAsync(selectedPRDs)) <= 0) 
            return;
        
        PokemonRenderDataItemsSource.Remove(selectedPRDs);
        PokemonRenderDataSelection.Clear();
    }
        
    [UsedImplicitly]
    public void SelectAllRenderData() => PokemonRenderDataSelection.SelectAll();
    [UsedImplicitly]
    public void DeselectAllRenderData() => PokemonRenderDataSelection.Clear();

    #endregion

    #region Render
    private CancellationTokenSource? renderCancelTokenSource;
    public ReactiveCommand<RenderTarget, Unit> RenderCommand { get; }
    private async Task Render(RenderTarget renderTarget)
    {
        DisposeCancelToken();
        renderCancelTokenSource = new CancellationTokenSource();
        NbOfPokemonRendered = 0;
        CurrentlyRendering = true;

        LogVM.WriteLine("------------------".AsMemory());
        Settings settings = await DoDBQueryAsync(db => db.GetSettingsAsync());
        IEnumerable<RenderJob> renderJobs = PokemonRenderDataSelection.SelectedItems.Where(prd => prd is not null).Select(prd => new RenderJob(prd!, settings, renderTarget));
        foreach (RenderJob job in renderJobs)
        {
            if (renderCancelTokenSource.IsCancellationRequested)
            {
                break;
            }

            try
            {
                async Task OnOutput(ReadOnlyMemory<char> output) => await Dispatcher.UIThread.InvokeAsync(() => LogVM.WriteLine(output));
                await job.RenderAsync(renderCancelTokenSource.Token, stepOutputAsync: OnOutput);
                NbOfPokemonRendered++;
            }
            catch (OperationCanceledException)
            {
                LogVM.WriteLine("Rendering cancelled.".AsMemory());
                PKXCore.Logger.Information("Rendering cancelled");
                break;
            }
            catch (Exception e)
            {
                LogVM.WriteLine("An error occured while rendering: Enable Blender logging, try to render again and see logs for details.".AsMemory());
                PKXCore.Logger.Error(e, "An error occured while rendering");
                break;
            }
        }

        EndRender();
    }
        
    [UsedImplicitly]
    public void EndRender()
    {
        DisposeCancelToken();
        CurrentlyRendering = false;
        LogVM.WriteLine("Rendering ended.".AsMemory());
        NameMap.CleanUp(); // Disposes handlers on the name map files
    }

    private void DisposeCancelToken()
    {
        // Make sure tokens are canceled
        if (renderCancelTokenSource == null) 
            return;
        
        renderCancelTokenSource.Cancel();
        renderCancelTokenSource.Dispose();
        renderCancelTokenSource = null;
    }
    #endregion

    public void Dispose()
    {
        DisposeCancelToken();
    }
}