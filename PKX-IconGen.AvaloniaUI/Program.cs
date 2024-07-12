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
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using PKXIconGen.Core;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace PKXIconGen.AvaloniaUI;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        PKXCore.Initiate();
        if (PKXCore.Initiated)
        {
            try
            {
                InitAvaloniaApp(args);
                return 0;
            }
            catch (Exception ex)
            {
                await HandleUnexpectedAppErrorAsync(ex);
                return 12;
            }
            finally
            {
                ReleaseApp();
            }
        }

        ReleaseApp();
        return 11;
    }

    private static void InitAvaloniaApp(string[] args)
    {
        PKXCore.Logger.Information("Initiating Avalonia App...");
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
    } 
    
    private static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<MaterialDesignIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    private static async Task HandleUnexpectedAppErrorAsync(Exception ex)
    {
        Settings? settings = null;
        try
        {
            Database db = Database.Instance;
            settings = await db.GetSettingsAsync();
        }
        catch (Exception settingsEx)
        {
            PKXCore.Logger.Fatal(settingsEx,
                "An exception occured while fetching settings on Avalonia exception");
        }

        PKXCore.Logger.Fatal(ex, "An unhandled exception occured. Settings used: {@Settings}",
            settings);
    }

    private static void ReleaseApp()
    {
        PKXCore.OnClose();
    }
}