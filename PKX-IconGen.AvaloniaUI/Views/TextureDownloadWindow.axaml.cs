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

using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PKXIconGen.AvaloniaUI.ViewModels;

namespace PKXIconGen.AvaloniaUI.Views
{
    public partial class TextureDownloadWindow : ReactiveWindow<TextureDownloadWindowViewModel>
    {
        public TextureDownloadWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closing += (sender, args) =>
            {
                args.Cancel = ((TextureDownloadWindowViewModel?)DataContext)?.Downloading ?? false;
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        
    }
}
