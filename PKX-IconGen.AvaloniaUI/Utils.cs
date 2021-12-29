using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;

namespace PKXIconGen.AvaloniaUI
{
    public static class Utils
    {
        public static IClassicDesktopStyleApplicationLifetime GetApplicationLifetime()
        {
            return Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ?? throw new System.InvalidOperationException();
        }
    }
}
