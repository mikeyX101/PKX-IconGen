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

using Avalonia.Controls;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.Services
{
    public static class DialogHelper
    {
        public static async Task<bool> ShowDialog(DialogType dialogType, DialogButtons dialogButtons, string message, string? title = null, Window? parent = null)
        {
            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            DialogWindowViewModel vm = new DialogWindowViewModel(dialogType, dialogButtons, message, title);
            await new DialogWindow
            {
                DataContext = vm
            }.ShowDialog(parent);

            return vm.ReturnValue;
        }

        public static async Task<bool> ShowDialog(string asset, DialogButtons dialogButtons, string message, string title, Window? parent = null)
        {
            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            DialogWindowViewModel vm = new DialogWindowViewModel(asset, dialogButtons, message, title);
            await new DialogWindow
            {
                DataContext = vm
            }.ShowDialog(parent);

            return vm.ReturnValue;
        }
    }
}
