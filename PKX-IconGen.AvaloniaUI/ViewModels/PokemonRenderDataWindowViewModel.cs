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
using System.Threading;
using JetBrains.Annotations;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Blender;
using PKXIconGen.Core.Services;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public sealed class PokemonRenderDataWindowViewModel : WindowViewModelBase, IDisposable
    {
        private readonly PokemonRenderData? data;
        private PokemonRenderData Data {
            get => data!;
            init {
                data = value;
                UpdateBindings();
            }
        }
        private bool showBox;
        private bool ShowBox
        {
            get => showBox;
            set
            {
                if (value)
                {
                    boxFrame = BoxAnimationFrame.First.GetBoxAnimation();
                }
                
                showBox = value;
                UpdateBindings();
            }
        }
        public BoxAnimation[] BoxAnimationItems { get; } = BoxAnimation.GetBoxAnimations();
        private BoxAnimation? boxFrame = null;
        public BoxAnimation BoxFrame
        {
            get => boxFrame ?? BoxAnimationItems.First(b => b.Frame == BoxAnimationFrame.First);
            set
            {
                boxFrame = value;
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
        private RenderData CurrentRenderData
        {
            get
            {
                RenderData renderData;
                if (ShowShiny)
                {
                    renderData = ShowBox ? Data.Shiny.BoxRender.GetBoxRenderData(BoxFrame.Frame) : Data.Shiny.FaceRender;
                }
                else
                {
                    renderData = ShowBox ? Data.BoxRender.GetBoxRenderData(BoxFrame.Frame) : Data.FaceRender;
                }
                return renderData;
            }
        }

        public string Title { get; init; }
        public IBlenderRunnerInfo BlenderRunnerInfo { get; init; }

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
            get => Data.Model;
            set {
                Data.Model = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Shiny
        public ShinyColor? Color1 {
            get => Data.Shiny.Color1;
            private set {
                Data.Shiny.Color1 = value;
                this.RaisePropertyChanged();
            }
        }
        public ShinyColor? Color2 {
            get => Data.Shiny.Color2;
            private set {
                Data.Shiny.Color2 = value;
                this.RaisePropertyChanged();
            }
        }
        public string? ShinyModel
        {
            get => Data.Shiny.Model;
            set {
                Data.Shiny.Model = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        public Color Background
        {
            get => CurrentRenderData.Background;
            set {
                CurrentRenderData.Background = value;
                this.RaisePropertyChanged();
            }
        }
        public Color Glow {
            get => CurrentRenderData.Glow;
            set {
                CurrentRenderData.Glow = value;
                this.RaisePropertyChanged();
            }
        }
        
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

            Data = renderData;
            UpdateBindings();

            // Reactive
            IObservable<bool> canModifyOrSave = this.WhenAnyValue(
                vm => vm.Name, vm => vm.Model, vm => vm.Color1, vm => vm.Color2, vm => vm.ShinyModel,
                (name, model, color1, color2, shinyModel) => 
                    !string.IsNullOrWhiteSpace(name) && 
                    !string.IsNullOrWhiteSpace(model) && (model.EndsWith(".dat") || model.EndsWith(".pkx")) && File.Exists(Core.Utils.GetTrueModelPath(model, BlenderRunnerInfo.AssetsPath)) &&
                    (   
                        color1.HasValue && color2.HasValue
                        ||
                        (!string.IsNullOrWhiteSpace(shinyModel) && (shinyModel.EndsWith(".dat") || shinyModel.EndsWith(".pkx")) && File.Exists(Core.Utils.GetTrueModelPath(shinyModel, BlenderRunnerInfo.AssetsPath)))
                    )
            );
            
            IObservable<bool> canSync = this.WhenAnyValue(
                vm => vm.Color1, vm => vm.Color2, vm => vm.Model,
                (color1, color2, model) => 
                    color1.HasValue && color2.HasValue &&
                    !string.IsNullOrWhiteSpace(model) && 
                    model.EndsWith(".pkx") && 
                    File.Exists(Core.Utils.GetTrueModelPath(model, BlenderRunnerInfo.AssetsPath))
            );

            ShinySyncCommand = ReactiveCommand.Create(ShinySync, canSync);
            ModifyBlenderDataCommand = ReactiveCommand.Create(ModifyBlenderData, canModifyOrSave);
            CancelCommand = ReactiveCommand.Create(Cancel);
            SaveCommand = ReactiveCommand.Create(Save, canModifyOrSave);
        }

        private void UpdateBindings()
        {
            this.RaisePropertyChanged(nameof(Name));
            this.RaisePropertyChanged(nameof(OutputName));
            this.RaisePropertyChanged(nameof(Model));

            this.RaisePropertyChanged(nameof(Color1));
            this.RaisePropertyChanged(nameof(Color2));
            this.RaisePropertyChanged(nameof(ShinyModel));

            this.RaisePropertyChanged(nameof(AnimationPose));
            this.RaisePropertyChanged(nameof(AnimationFrame));

            this.RaisePropertyChanged(nameof(MainCamera));
            this.RaisePropertyChanged(nameof(MainLight));
            this.RaisePropertyChanged(nameof(SecondaryCamera));
            this.RaisePropertyChanged(nameof(SecondaryLight));

            this.RaisePropertyChanged(nameof(RemovedObjects));
            
            this.RaisePropertyChanged(nameof(Background));
            this.RaisePropertyChanged(nameof(Glow));
        }
        
        #region Modify in Blender
        private CancellationTokenSource? modifyBlenderDataCancelTokenSource;
        public ReactiveCommand<Unit, Unit> ModifyBlenderDataCommand { get; }
        private async void ModifyBlenderData()
        {
            DisposeCancelModifyDataToken();
            modifyBlenderDataCancelTokenSource = new CancellationTokenSource();

            CurrentlyModifying = true;
            try
            {
                await Data.ModifyAsync(BlenderRunnerInfo, modifyBlenderDataCancelTokenSource.Token, onFinish: EndModifyBlenderData);
            }
            catch (Exception e)
            {
                await DialogHelper.ShowDialog(DialogType.Error, DialogButtons.Ok, "An error occured, reason below and details in logs.\n" + e.Message);
                EndModifyBlenderData();
            }
        }
        [UsedImplicitly]
        public void EndModifyBlenderData()
        {
            UpdateBindings();
            
            DisposeCancelModifyDataToken();
            CurrentlyModifying = false;
        }
        private void DisposeCancelModifyDataToken()
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
                Color1 = ShinyColor.GetDefaultShinyColor1();
                Color2 = ShinyColor.GetDefaultShinyColor2();
                ShinyModel = null;
                
                UpdateBindings();
            }
        }
        
        [UsedImplicitly]
        public void UseShinyModel()
        {
            if (Color1.HasValue && Color2.HasValue)
            {
                ShinyModel = "";
                Color1 = null;
                Color2 = null;
                
                UpdateBindings();
            }
        }
        #endregion
        
        #region Other Buttons
        public ReactiveCommand<Unit, Unit> ShinySyncCommand { get; }
        private async void ShinySync()
        {
            // Model should be valid here
            try
            {
                await using ShinyExtractor shiny = new(Core.Utils.GetTrueModelPath(Model, BlenderRunnerInfo.AssetsPath)!);

                ShinyColor[]? colors = shiny.GetColors();
                if (colors is not null)
                {
                    await DialogHelper.ShowDialog(DialogType.Info, DialogButtons.Ok, $"Colors found!\nColor1: {colors[0].DisplayString}\nColor2: {colors[1].DisplayString}");

                    Color1 = colors[0];
                    Color2 = colors[1];
                }
                else
                {
                    await DialogHelper.ShowDialog(DialogType.Warning, DialogButtons.Ok, "This game file doesn't support shiny colors.");
                }
            }
            catch (Exception)
            {
                await DialogHelper.ShowDialog(DialogType.Warning, DialogButtons.Ok, "No colors found in this file.");
            }
        }
        
        public void ShinyToggle() => ShowShiny = !ShowShiny;
        public void BoxToggle() => ShowBox = !ShowBox;
        public ReactiveCommand<Unit, object> CancelCommand { get; }
        private static object Cancel() => false;
        public ReactiveCommand<Unit, object> SaveCommand { get; }
        private static object Save() => true;
        #endregion

        public void Dispose()
        {
            DisposeCancelModifyDataToken();
        }
    }
}
