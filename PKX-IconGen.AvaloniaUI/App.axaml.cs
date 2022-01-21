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

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;
using PKXIconGen.Core;
using PKXIconGen.Core.Services;
using Projektanker.Icons.Avalonia;
using System;
using System.Linq;
using AvaloniaColorPicker;

namespace PKXIconGen.AvaloniaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Database db = Database.Instance;
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = new MainWindowViewModel(await db.GetSettingsAsync(), await db.GetPokemonRenderDataAsync())
                };
            }
            else
            {
                // Work around for other assemblies for the designer (https://github.com/AvaloniaUI/Avalonia/issues/7126)
                GC.KeepAlive(typeof(Interaction));
                GC.KeepAlive(typeof(InvokeCommandAction));

                GC.KeepAlive(typeof(Icon));
                GC.KeepAlive(typeof(Attached));

                GC.KeepAlive(typeof(ColorPicker));
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
