﻿#region License
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
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using PKXIconGen.AvaloniaUI.ViewModels;
using PKXIconGen.AvaloniaUI.Views;

namespace PKXIconGen.AvaloniaUI.Services;

public static class DialogHelper
{
    private static readonly Dictionary<Type, Type> windowTypeCache = new();

    public static async Task<bool> ShowDialog(DialogType dialogType, DialogButtons dialogButtons, string message, uint? height = null, string? title = null, Window? parent = null)
    {
        parent ??= Utils.GetApplicationLifetime().MainWindow ?? throw new InvalidOperationException("MainWindow was null.");

        DialogWindowViewModel vm = new(dialogType, dialogButtons, message, height, title);
        return await new DialogWindow
        {
            DataContext = vm
        }.ShowDialog<bool>(parent);
    }

    public static async Task<bool> ShowDialog(string asset, DialogButtons dialogButtons, string message, string title, uint? height = null, Window? parent = null)
    {
        parent ??= Utils.GetApplicationLifetime().MainWindow ?? throw new InvalidOperationException("MainWindow was null.");

        DialogWindowViewModel vm = new(asset, dialogButtons, message, title, height);
        return await new DialogWindow
        {
            DataContext = vm
        }.ShowDialog<bool>(parent);
    }

    public static async Task ShowWindowDialog<TViewModel>(TViewModel vm, Window? parent = null) where TViewModel : ViewModelBase
    {
        await ShowWindowDialog<TViewModel, Unit>(vm, parent);
    }
        
    public static async Task<TResult> ShowWindowDialog<TViewModel, TResult>(TViewModel vm, Window? parent = null) where TViewModel : ViewModelBase
    {
        Type vmType = typeof(TViewModel);
        if (!windowTypeCache.TryGetValue(vmType, out Type? value))
        {
            if (vmType.FullName == null)
            {
                Exception ex = new NullReferenceException("vmType.FullName is null.");
                Core.PKXCore.Logger.Error(ex, "vmType.FullName is null");
                throw ex;
            }

            Type? windowType = Type.GetType(vmType.FullName.Remove(vmType.FullName.LastIndexOf("ViewModel", StringComparison.InvariantCulture)).Replace(".ViewModels.", ".Views."));
            if (windowType == null)
            {
                Exception ex = new NullReferenceException($"No view type were found for {typeof(TViewModel)}");
                Core.PKXCore.Logger.Error(ex, "No view type were found for {@ViewModelType}", typeof(TViewModel));
                throw ex;
            }
            if (!windowType.IsAssignableTo(typeof(Window)))
            {
                Exception ex = new InvalidCastException("Found window type is not assignable to Window.");
                Core.PKXCore.Logger.Error(ex, "Found window type {@WindowType} is not assignable to Window", windowType.Name);
                throw ex;
            }

            value = windowType;
            windowTypeCache.Add(vmType, value);
        }
            
        Window? window = (Window?)Activator.CreateInstance(value);
        if (window == null)
        {
            Exception ex = new NullReferenceException("Window was null.");
            Core.PKXCore.Logger.Error(ex, "Window was null");
            throw ex;
        }
        
        window.DataContext = vm;
        parent ??= Utils.GetApplicationLifetime().MainWindow ?? throw new InvalidOperationException("MainWindow was null.");
        return await window.ShowDialog<TResult>(parent);
    }
}