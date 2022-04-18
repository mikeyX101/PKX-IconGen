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
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Blender;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.IO;
using PKXIconGen.Core.Services;
using System.Threading;
using System.Numerics;
using PKXIconGen.Core;
using System.Reactive.Disposables;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class PokemonRenderDataWindowViewModel : WindowViewModelBase
    {


        public string Title { get; init; }

        #region General
        private string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string? outputName;
        public string? OutputName
        {
            get => outputName;
            set => this.RaiseAndSetIfChanged(ref outputName, value);
        }

        private string model;
        public string Model
        {
            get => model;
            set => this.RaiseAndSetIfChanged(ref model, value);
        }
        #endregion

        #region Shiny
        
        private Color? shinyColor;
        public Color? ShinyColor
        {
            get => shinyColor;
            set
            {
                this.RaiseAndSetIfChanged(ref shinyColor, value);
            }
        }
        private string? shinyModel;
        public string ShinyModel
        {
            get => shinyModel ?? "";
            set => this.RaiseAndSetIfChanged(ref shinyModel, value);
        }
        #endregion

        #region Blender Data
        private ushort normalAnimPose;
        public ushort NormalAnimPose {
            get => normalAnimPose;
            set => this.RaiseAndSetIfChanged(ref normalAnimPose, value);
        }
        private ushort normalAnimFrame;
        public ushort NormalAnimFrame
        {
            get => normalAnimFrame;
            set => this.RaiseAndSetIfChanged(ref normalAnimFrame, value);
        }

        private ushort shinyAnimPose;
        public ushort ShinyAnimPose
        {
            get => shinyAnimPose;
            set => this.RaiseAndSetIfChanged(ref shinyAnimPose, value);
        }
        private ushort shinyAnimFrame;
        public ushort ShinyAnimFrame
        {
            get => shinyAnimFrame;
            set => this.RaiseAndSetIfChanged(ref shinyAnimFrame, value);
        }

        private Camera mainCamera;
        public Camera MainCamera
        {
            get => mainCamera;
            set => this.RaiseAndSetIfChanged(ref mainCamera, value);
        }
        public IList<Light> MainLights { get; set; }
        private Camera? secondaryCamera;
        public Camera? SecondaryCamera {
            get => secondaryCamera;
            set => this.RaiseAndSetIfChanged(ref secondaryCamera, value);
        }
        public IList<Light> SecondaryLights { get; set; }

        private IList<string> removedObjects;
        public IList<string> RemovedObjects
        {
            get => removedObjects;
            set => this.RaiseAndSetIfChanged(ref removedObjects, value);
        }
        #endregion

        public bool currentlyModifying = false;
        public bool CurrentlyModifying
        {
            get => currentlyModifying;
            set => this.RaiseAndSetIfChanged(ref currentlyModifying, value);
        }

        private IBlenderRunnerInfo BlenderRunnerInfo { get; }
        private bool useFilter;
        public bool UseFilter
        {
            get => useFilter;
            set => this.RaiseAndSetIfChanged(ref useFilter, value);
        }

#pragma warning disable CS8618
        public PokemonRenderDataWindowViewModel(string title, IBlenderRunnerInfo blenderRunnerInfo, PokemonRenderData? renderData)
#pragma warning restore CS8618
        {
            Title = title;
            useFilter = true;
            BlenderRunnerInfo = blenderRunnerInfo;

            PopulateViewModel(renderData ?? new());

            // Reactive
            IObservable<bool> canModifyOrSave = this.WhenAnyValue(
                vm => vm.Name, vm => vm.Model, vm => vm.ShinyModel,
                (name, model, shinyModel) => 
                    !string.IsNullOrWhiteSpace(name) && 
                    !string.IsNullOrWhiteSpace(model) && 
                    File.Exists(model) &&
                    (   
                        string.IsNullOrWhiteSpace(shinyModel) 
                        ||
                        File.Exists(shinyModel) 
                    )
            );

            ModifyBlenderDataCommand = ReactiveCommand.Create(ModifyBlenderData, canModifyOrSave);
            CancelCommand = ReactiveCommand.Create(Cancel);
            SaveCommand = ReactiveCommand.Create(Save, canModifyOrSave);
        }

        public void PopulateViewModel(PokemonRenderData renderData)
        {
            useFilter = renderData.Shiny.Filter.HasValue;
            
            name = renderData.Name;
            outputName = renderData.OutputName;
            model = renderData.Render.Model;

            shinyColor = renderData.Shiny.Filter;
            shinyModel = renderData.Shiny.Render.Model;

            normalAnimPose = renderData.Render.AnimationPose;
            normalAnimFrame = renderData.Render.AnimationFrame;

            shinyAnimPose = renderData.Shiny.Render.AnimationPose;
            shinyAnimFrame = renderData.Shiny.Render.AnimationFrame;

            MainCamera = renderData.Render.MainCamera;
            MainLights = new ObservableCollection<Light>(renderData.Render.MainLights);
            SecondaryCamera = renderData.Render.SecondaryCamera;
            SecondaryLights = new ObservableCollection<Light>(renderData.Render.SecondaryLights);

            removedObjects = new ObservableCollection<string>(renderData.RemovedObjects);
        }

        public async void BrowseModelPath()
        {
            string? path = await SelectModel();
            if (path != null)
            {
                Model = path;
            }
        }

        public async void BrowseShinyModelPath()
        {
            string? path = await SelectModel();
            if (path != null)
            {
                ShinyModel = path;
            }
        }

        private async Task<string?> SelectModel()
        {
            List<FileDialogFilter>? filters = null;
            if (IsWindows)
            {
                filters = new();
                List<string> extensions = new()
                {
                    "dat"
                };
                filters.Add(new FileDialogFilter { Extensions = extensions, Name = "GCN/WII Pokemon Model" });
            }

            return await FileDialogHelper.GetFile("Select a model...", filters);
        }

        #region Modify in Blender
        private CancellationTokenSource? modifyBlenderDataCancelTokenSource = null;
        public ReactiveCommand<Unit, Unit> ModifyBlenderDataCommand { get; }
        public async void ModifyBlenderData()
        {
            DisposeCancelToken();
            modifyBlenderDataCancelTokenSource = new();

            IBlenderRunner blenderRunner = BlenderRunners.GetModifyDataRunner(BlenderRunnerInfo, GetPokemonRenderData());
            blenderRunner.OnFinish += EndModifyBlenderData;

            CurrentlyModifying = true;
            await blenderRunner.RunAsync(modifyBlenderDataCancelTokenSource.Token);
        }
        public void EndModifyBlenderData(PokemonRenderData? newPrd)
        {
            if (newPrd is not null)
            {
                PopulateViewModel(newPrd);
            }

            DisposeCancelToken();
            CurrentlyModifying = false;
        }
        public void DisposeCancelToken()
        {
            // Make sure tokens are canceled
            if (modifyBlenderDataCancelTokenSource != null)
            {
                modifyBlenderDataCancelTokenSource.Cancel();
                modifyBlenderDataCancelTokenSource.Dispose();
                modifyBlenderDataCancelTokenSource = null;
            }
        }
        #endregion

        #region Data
        public PokemonRenderData GetPokemonRenderData() => 
            new(
                Name,
                OutputName,
                new RenderData(Model, NormalAnimPose, NormalAnimFrame, MainCamera, MainLights.ToArray(), SecondaryCamera, SecondaryLights.ToArray()),
                UseFilter ? 
                    new ShinyInfo(ShinyColor, new RenderData(Model, ShinyAnimPose, ShinyAnimFrame, MainCamera, MainLights.ToArray(), SecondaryCamera, SecondaryLights.ToArray())) : 
                    new ShinyInfo(new RenderData(ShinyModel, ShinyAnimPose, ShinyAnimFrame, MainCamera, MainLights.ToArray(), SecondaryCamera, SecondaryLights.ToArray())),
                RemovedObjects.ToHashSet()
            );
        #endregion

        #region Save
        public ReactiveCommand<Unit, object?> CancelCommand { get; }
        public static object? Cancel() => null;
        public ReactiveCommand<Unit, PokemonRenderData> SaveCommand { get; }
        public PokemonRenderData Save() => GetPokemonRenderData();
        #endregion
    }
}
