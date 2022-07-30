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
using Avalonia.Media;
using AvaloniaColorPicker;
using JetBrains.Annotations;
using AvaloniaColor = Avalonia.Media.Color;

namespace PKXIconGen.AvaloniaUI.Views.Controls
{
    public partial class ColorDisplay
    {
        [UsedImplicitly] 
        public static readonly StyledProperty<AvaloniaColor> ColorProperty =
            AvaloniaProperty.Register<ColorDisplay, AvaloniaColor>(nameof(Color), defaultBindingMode: BindingMode.TwoWay);

        public AvaloniaColor Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
    }
}