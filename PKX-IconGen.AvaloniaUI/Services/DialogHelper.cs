#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
    Copyright (C) 2021-2022 mikeyX#4697

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
