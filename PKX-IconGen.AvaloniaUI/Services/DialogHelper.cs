using Avalonia.Controls;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.Services
{
    public static class DialogHelper
    {
        public static void ShowDialog(DialogType dialogType, string message, string? title = null, Window? parent = null)
        {
            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            new DialogWindow
            {
                DataContext = new DialogWindowViewModel(dialogType, message, title)
            }.ShowDialog(parent);
        }

        public static void ShowDialog(string asset, string message, string title, Window? parent = null)
        {
            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            new DialogWindow
            {
                DataContext = new DialogWindowViewModel(asset, message, title)
            }.ShowDialog(parent);
        }
    }
}
