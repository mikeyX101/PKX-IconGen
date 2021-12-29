using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactivity;
using Avalonia.Xaml.Interactions.Core;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;
using PKXIconGen.Core.Services;
using Projektanker.Icons.Avalonia;
using System;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core;

namespace PKXIconGen.AvaloniaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += CoreManager.OnApplicationEnd;

                using (Database db = new())
                {
                    desktop.MainWindow = new MainWindow()
                    {
                        DataContext = new MainWindowViewModel(db.GetSettings())
                    };
                }
            }
            else
            {
                // Work around for other assemblies for the designer (https://github.com/AvaloniaUI/Avalonia/issues/7126)
                GC.KeepAlive(typeof(Interaction));
                GC.KeepAlive(typeof(InvokeCommandAction));

                GC.KeepAlive(typeof(Icon));
                GC.KeepAlive(typeof(Attached));
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
