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
using System;
using System.Reflection;
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

            DialogWindowViewModel vm = new(dialogType, dialogButtons, message, title);
            return await new DialogWindow
            {
                DataContext = vm
            }.ShowDialog<bool>(parent);
        }

        public static async Task<bool> ShowDialog(string asset, DialogButtons dialogButtons, string message, string title, Window? parent = null)
        {
            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            DialogWindowViewModel vm = new(asset, dialogButtons, message, title);
            return await new DialogWindow
            {
                DataContext = vm
            }.ShowDialog<bool>(parent);
        }

        public static async Task<TResult> ShowWindowDialog<TViewModel, TResult>(TViewModel vm, Window? parent = null) where TViewModel : ViewModelBase
        {
            Type vmType = typeof(TViewModel);
            if (vmType.FullName == null)
            {
                Exception ex = new NullReferenceException("vmType.FullName is null.");
                Core.CoreManager.Logger.Error(ex, ex.Message);
                throw ex;
            }

            Type? windowType = Type.GetType(vmType.FullName.Remove(vmType.FullName.LastIndexOf("ViewModel")).Replace(".ViewModels.", ".Views."));
            if (windowType == null)
            {
                Exception ex = new NullReferenceException($"No view type were found for {typeof(TViewModel)}");
                Core.CoreManager.Logger.Error(ex, ex.Message);
                throw ex;
            }
            else if (!windowType.IsAssignableTo(typeof(Window)))
            {
                Exception ex = new InvalidCastException("Passed type is not assignable to Window.");
                Core.CoreManager.Logger.Error(ex, ex.Message);
                throw ex;
            }

            if (parent == null)
            {
                parent = Utils.GetApplicationLifetime().MainWindow;
            }

            Window? window = (Window?)Activator.CreateInstance(windowType);
            if (window != null)
            {
                window.DataContext = vm;

                return await window.ShowDialog<TResult>(parent);
            }
            else
            {
                Exception ex = new NullReferenceException("Window was null.");
                Core.CoreManager.Logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}
