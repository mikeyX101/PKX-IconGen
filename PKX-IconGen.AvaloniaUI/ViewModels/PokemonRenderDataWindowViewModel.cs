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
using JetBrains.Annotations;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class PokemonRenderDataWindowViewModel : WindowViewModelBase
    {
        private PokemonRenderData data;
        private PokemonRenderData Data {
            get => data;
            set {
                data = value;
                UpdateBindings();
            }
        }
        private bool showShiny;
        private bool ShowShiny
        {
            get => showShiny;
            set
            {
                showShiny = value;
                UpdateBindings();
            }
        }
        private RenderData CurrentRenderData => ShowShiny ? Data.Shiny.Render : Data.Render;

        public string Title { get; init; }
        private IBlenderRunnerInfo BlenderRunnerInfo { get; init; }

        #region General
        public string Name
        {
            get => Data.Name;
            set {
                Data.Name = value;
                this.RaisePropertyChanged();
            }
        }

        public string? OutputName
        {
            get => Data.OutputName;
            set {
                Data.OutputName = value;
                this.RaisePropertyChanged();
            }
        }

        public string Model
        {
            get => Data.Render.Model;
            set {
                Data.Render.Model = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Shiny
        public Color? ShinyColor
        {
            get => Data.Shiny.Filter;
            set {
                Data.Shiny.Filter = value;
                this.RaisePropertyChanged();
            }
        }
        public string ShinyModel
        {
            get => Data.Shiny.Render.Model;
            set {
                Data.Shiny.Render.Model = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Blender Data
        public ushort AnimationPose => CurrentRenderData.AnimationPose;
        public ushort AnimationFrame => CurrentRenderData.AnimationFrame;
        public Camera MainCamera => CurrentRenderData.MainCamera;
        public Light MainLight => CurrentRenderData.MainCamera.Light;
        public Camera? SecondaryCamera => CurrentRenderData.SecondaryCamera;
        public Light? SecondaryLight => CurrentRenderData.SecondaryCamera?.Light;

        public ISet<string> RemovedObjects => CurrentRenderData.RemovedObjects;
        #endregion

        private bool currentlyModifying;
        public bool CurrentlyModifying
        {
            get => currentlyModifying;
            set => this.RaiseAndSetIfChanged(ref currentlyModifying, value);
        }

        public PokemonRenderDataWindowViewModel(string title, IBlenderRunnerInfo blenderRunnerInfo, PokemonRenderData? renderData)
        {
            Title = title;

            BlenderRunnerInfo = blenderRunnerInfo;

            data = renderData ?? new();
            UpdateBindings();

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
            ShinyToggleCommand = ReactiveCommand.Create(ShinyToggle);
            CancelCommand = ReactiveCommand.Create(Cancel);
            SaveCommand = ReactiveCommand.Create(Save, canModifyOrSave);
        }

        private void UpdateBindings()
        {
            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(OutputName));
            this.RaisePropertyChanged(nameof(Model));

            this.RaisePropertyChanged(nameof(ShinyColor));
            this.RaisePropertyChanged(nameof(ShinyModel));

            this.RaisePropertyChanged(nameof(AnimationPose));
            this.RaisePropertyChanged(nameof(AnimationFrame));

            this.RaisePropertyChanged(nameof(MainCamera));
            this.RaisePropertyChanged(nameof(MainLight));
            this.RaisePropertyChanged(nameof(SecondaryCamera));
            this.RaisePropertyChanged(nameof(SecondaryLight));

            this.RaisePropertyChanged(nameof(RemovedObjects));
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
        private CancellationTokenSource? modifyBlenderDataCancelTokenSource;
        [UsedImplicitly]
        public ReactiveCommand<Unit, Unit> ModifyBlenderDataCommand { get; }
        private async void ModifyBlenderData()
        {
            DisposeCancelToken();
            modifyBlenderDataCancelTokenSource = new();

            CurrentlyModifying = true;
            await Data.ModifyAsync(BlenderRunnerInfo, modifyBlenderDataCancelTokenSource.Token, onFinish: EndModifyBlenderData);
        }
        private void EndModifyBlenderData(PokemonRenderData? newPrd)
        {
            if (newPrd is not null)
            {
                Data = newPrd;
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

        #region Other Buttons
        [UsedImplicitly]
        public ReactiveCommand<Unit, Unit> ShinyToggleCommand { get; }
        private void ShinyToggle() => ShowShiny = !ShowShiny;
        [UsedImplicitly]
        public ReactiveCommand<Unit, object?> CancelCommand { get; }
        private static object? Cancel() => null;
        [UsedImplicitly]
        public ReactiveCommand<Unit, PokemonRenderData> SaveCommand { get; }
        private PokemonRenderData Save() => Data;
        #endregion
    }
}
