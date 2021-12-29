using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using PKXIconGen.Core;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Services;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using System.Diagnostics;

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

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
