#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
    Copyright (C) 2021-2024 Samuel Caron/mikeyX#4697

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

using System.Reactive;
using PKXIconGen.Core.Data;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels;

public sealed class SettingsWindowViewModel : WindowViewModelBase
{
    #region Settings
    private bool logBlender;
    public bool LogBlender
    {
        get => logBlender;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.LogBlender, value));
            this.RaiseAndSetIfChanged(ref logBlender, value); 
        }
    }
        
    private bool saturationBoost;
    public bool SaturationBoost
    {
        get => saturationBoost;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.SaturationBoost, value));
            this.RaiseAndSetIfChanged(ref saturationBoost, value); 
        }
    }
        
    private bool saveDanceGIF;
    public bool SaveDanceGIF
    {
        get => saveDanceGIF;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.SaveDanceGIF, value));
            this.RaiseAndSetIfChanged(ref saveDanceGIF, value); 
        }
    }
        
    private bool devTools;
    public bool DevTools
    {
        get => devTools;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.DevTools, value));
            this.RaiseAndSetIfChanged(ref devTools, value); 
        }
    }
        
    private Game outputNameForGame;
    public Game OutputNameForGame
    {
        get => outputNameForGame;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.OutputNameForGame, value));
            this.RaiseAndSetIfChanged(ref outputNameForGame, value); 
            this.RaisePropertyChanged(nameof(OutputForTargetEnabled));
        }
    }
    // TODO Replace these by ObjectConverter.Equals once 0.11.1 is out
    public bool IsGameUndefined => OutputNameForGame == Game.Undefined;
    public bool IsGameColo => OutputNameForGame == Game.PokemonColosseum;
    public bool IsGameXD => OutputNameForGame == Game.PokemonXDGaleOfDarkness;
        
    private TextureTargetChoice outputNameForTarget;
    public TextureTargetChoice OutputNameForTarget
    {
        get => outputNameForTarget;
        set { 
            DoDBQuery(db => db.SaveSettingsProperty(s => s.OutputNameForTarget, value));
            this.RaiseAndSetIfChanged(ref outputNameForTarget, value); 
        }
    }
    // TODO Replace these by ObjectConverter.Equals once 0.11.1 is out
    public bool IsTargetOriginal => OutputNameForTarget == TextureTargetChoice.Original;
    public bool IsTargetDolphin => OutputNameForTarget == TextureTargetChoice.Dolphin;
    #endregion

    public bool OutputForTargetEnabled => OutputNameForGame != Game.Undefined;
        
    public SettingsWindowViewModel(Settings settings)
    {
        LogBlender = settings.LogBlender;
        SaturationBoost = settings.SaturationBoost;
        SaveDanceGIF = settings.SaveDanceGIF;
        DevTools = settings.DevTools;
        OutputNameForTarget = settings.OutputNameForTarget;
        OutputNameForGame = settings.OutputNameForGame;
            
        ChangeOutputForGameCommand = ReactiveCommand.Create<Game>(ChangeOutputNameForGame);
        ChangeOutputForTargetCommand = ReactiveCommand.Create<TextureTargetChoice>(ChangeOutputNameForTarget);
    }
        
    public ReactiveCommand<Game, Unit> ChangeOutputForGameCommand { get; }
    private void ChangeOutputNameForGame(Game game)
    {
        OutputNameForGame = game;
    }
        
    public ReactiveCommand<TextureTargetChoice, Unit> ChangeOutputForTargetCommand { get; }
    private void ChangeOutputNameForTarget(TextureTargetChoice target)
    {
        OutputNameForTarget = target;
    }
}