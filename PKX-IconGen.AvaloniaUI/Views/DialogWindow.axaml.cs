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

using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using PKXIconGen.AvaloniaUI.ViewModels;

namespace PKXIconGen.AvaloniaUI.Views;

public partial class DialogWindow : ReactiveWindow<DialogWindowViewModel>
{
    public DialogWindow()
    {
        InitializeComponent();
    }

    public void CloseFalse(object sender, RoutedEventArgs e)
    {
        Close(false);
    }
    public void CloseTrue(object sender, RoutedEventArgs e)
    {
        Close(true);
    }
}