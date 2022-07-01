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
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using JetBrains.Annotations;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Blender;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public sealed class PokemonRenderDataWindowViewModel : WindowViewModelBase, IDisposable
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

        public string? Model
        {
            get => Data.Render.Model;
            set {
                Data.Render.Model = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Shiny
        public float? ShinyHue
        {
            get => Data.Shiny.Hue;
            set {
                Data.Shiny.Hue = value;
                this.RaisePropertyChanged();
            }
        }
        public string? ShinyModel
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

        public PokemonRenderDataWindowViewModel(string title, IBlenderRunnerInfo blenderRunnerInfo, PokemonRenderData renderData)
        {
            Title = title;

            BlenderRunnerInfo = blenderRunnerInfo;

            data = renderData;
            UpdateBindings();

            // Reactive
            IObservable<bool> canModifyOrSave = this.WhenAnyValue(
                vm => vm.Name, vm => vm.Model, vm => vm.ShinyHue, vm => vm.ShinyModel,
                (name, model, shinyHue, shinyModel) => 
                    !string.IsNullOrWhiteSpace(name) && 
                    !string.IsNullOrWhiteSpace(model) && model.EndsWith(".dat") && File.Exists(Core.Utils.GetTrueModelPath(model, BlenderRunnerInfo.AssetsPath)) &&
                    (   
                        shinyHue.HasValue
                        ||
                        (!string.IsNullOrWhiteSpace(shinyModel) && shinyModel.EndsWith(".dat") && File.Exists(Core.Utils.GetTrueModelPath(shinyModel, BlenderRunnerInfo.AssetsPath)))
                    )
            );

            ModifyBlenderDataCommand = ReactiveCommand.Create(ModifyBlenderData, canModifyOrSave);
            CancelCommand = ReactiveCommand.Create(Cancel);
            SaveCommand = ReactiveCommand.Create(Save, canModifyOrSave);
        }

        private void UpdateBindings()
        {
            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(OutputName));
            this.RaisePropertyChanged(nameof(Model));

            this.RaisePropertyChanged(nameof(ShinyHue));
            this.RaisePropertyChanged(nameof(ShinyModel));

            this.RaisePropertyChanged(nameof(AnimationPose));
            this.RaisePropertyChanged(nameof(AnimationFrame));

            this.RaisePropertyChanged(nameof(MainCamera));
            this.RaisePropertyChanged(nameof(MainLight));
            this.RaisePropertyChanged(nameof(SecondaryCamera));
            this.RaisePropertyChanged(nameof(SecondaryLight));

            this.RaisePropertyChanged(nameof(RemovedObjects));
        }

        [UsedImplicitly]
        public async void BrowseModelPath()
        {
            string? path = await SelectModel();
            if (path != null)
            {
                Model = path;
            }
        }

        [UsedImplicitly]
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
                filters = new List<FileDialogFilter>();
                List<string> extensions = new()
                {
                    "dat"
                };
                filters.Add(new FileDialogFilter { Extensions = extensions, Name = "GCN/WII Pokemon Model" });
            }

            return await FileDialogHelper.GetFile("Select a model...", filters, !string.IsNullOrWhiteSpace(Model) ? Model : null);
        }

        [UsedImplicitly]
        public void InsertAssetsPath(TextBox textBox)
        {
            const string toInsert = "{{AssetsPath}}";

            textBox.Text = textBox.Text is null ? toInsert : textBox.Text.Insert(textBox.CaretIndex, toInsert);
            textBox.Focus();
            textBox.CaretIndex += toInsert.Length;
        }

        #region Modify in Blender
        private CancellationTokenSource? modifyBlenderDataCancelTokenSource;
        [UsedImplicitly]
        public ReactiveCommand<Unit, Unit> ModifyBlenderDataCommand { get; }
        private async void ModifyBlenderData()
        {
            DisposeCancelRenderToken();
            modifyBlenderDataCancelTokenSource = new CancellationTokenSource();

            CurrentlyModifying = true;
            await Data.ModifyAsync(BlenderRunnerInfo, modifyBlenderDataCancelTokenSource.Token, onFinish: EndModifyBlenderData);
        }
        private void EndModifyBlenderData()
        {
            UpdateBindings();
            
            DisposeCancelRenderToken();
            CurrentlyModifying = false;
        }
        private void DisposeCancelRenderToken()
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

        #region Shiny
        [UsedImplicitly]
        public void UseShinyColor()
        {
            if (ShinyModel != null)
            {
                ShinyHue = 1;
                ShinyModel = null;
            }
        }
        
        [UsedImplicitly]
        public void UseShinyModel()
        {
            if (ShinyHue != null)
            {
                ShinyModel = "";
                ShinyHue = null;
            }
        }
        #endregion
        
        #region Other Buttons
        [UsedImplicitly]
        public void ShinyToggle() => ShowShiny = !ShowShiny;
        [UsedImplicitly]
        public ReactiveCommand<Unit, object> CancelCommand { get; }
        private static object Cancel() => false;
        [UsedImplicitly]
        public ReactiveCommand<Unit, object> SaveCommand { get; }

        private static object Save() => true;
        #endregion

        public void Dispose()
        {
            DisposeCancelRenderToken();
        }
    }
}
