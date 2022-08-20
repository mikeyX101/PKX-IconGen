#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
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
using Avalonia.Data;
using JetBrains.Annotations;
using PKXIconGen.AvaloniaUI.Models;

namespace PKXIconGen.AvaloniaUI.Views.Controls {

    public partial class FileTextField
    {
        [UsedImplicitly] 
        public static readonly StyledProperty<bool> IsAssetsPathFieldProperty =
            AvaloniaProperty.Register<FileTextField, bool>(nameof(IsAssetsPathField), defaultBindingMode: BindingMode.OneTime);
        
        [UsedImplicitly] 
        public static readonly StyledProperty<string> PathProperty =
            AvaloniaProperty.Register<FileTextField, string>(nameof(Path), defaultBindingMode: BindingMode.TwoWay, defaultValue: "");
        
        [UsedImplicitly] 
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<FileTextField, string>(nameof(Title), defaultBindingMode: BindingMode.OneTime, inherits: true);
        
        [UsedImplicitly] 
        public static readonly StyledProperty<FileSelectType> TypeProperty =
            AvaloniaProperty.Register<FileTextField, FileSelectType>(nameof(Type), defaultBindingMode: BindingMode.OneTime, inherits: true);

        [UsedImplicitly] 
        public static readonly StyledProperty<string> AssetsPathProperty =
            AvaloniaProperty.Register<FileTextField, string>(nameof(AssetsPath), defaultBindingMode: BindingMode.OneTime, inherits: true);
        
        public bool IsAssetsPathField
        {
            get => GetValue(IsAssetsPathFieldProperty);
            set => SetValue(IsAssetsPathFieldProperty, value);
        }
        
        public string Path
        {
            get => GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }
        
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        
        public FileSelectType Type
        {
            get => GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }
        
        public string AssetsPath
        {
            get => GetValue(AssetsPathProperty);
            set => SetValue(AssetsPathProperty, value);
        }
    }
}