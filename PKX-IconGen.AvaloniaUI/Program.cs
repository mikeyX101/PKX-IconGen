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
using Avalonia.ReactiveUI;
using PKXIconGen.Core;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;

namespace PKXIconGen.AvaloniaUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            IconProvider.Register<FontAwesomeIconProvider>();
            CoreManager.Initiate();
            if (CoreManager.Initiated)
            {
                try
                {
                    CoreManager.Logger.Information("Initiating Avalonia App...");
                    BuildAvaloniaApp()
                        .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
                }
                catch (Exception ex)
                {
                    Settings? settings = null;
                    try
                    {
                        using Database db = new();
                        settings = db.GetSettings();
                    }
                    catch (Exception settingsEx)
                    {
                        CoreManager.Logger.Fatal(settingsEx, "An exception occured while fetching settings on Avalonia exception.");
                    }

                    CoreManager.Logger.Fatal(ex, "An unhandled exception occured. Settings used: {@Settings}", settings);
                }
                finally
                {
                    CoreManager.DisposeLogger();
                }
            }
            else
            {
                CoreManager.DisposeLogger();
            }
    }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
