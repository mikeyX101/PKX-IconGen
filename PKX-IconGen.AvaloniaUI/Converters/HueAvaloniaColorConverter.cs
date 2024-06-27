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
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;

namespace PKXIconGen.AvaloniaUI.Converters;

public class HueAvaloniaColorConverter : IValueConverter
{
    public static readonly HueAvaloniaColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is float hue && targetType.IsAssignableFrom(typeof(AvaloniaColor)))
        {
            uint rgb = Core.Utils.HueToRgb(Core.Utils.ConvertRange(0, 1, 0, 360, hue));
            return AvaloniaColor.FromUInt32(rgb);
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new Avalonia.Data.BindingNotification(
            new NotSupportedException("Converting a Avalonia color back to the original hue is not supported."),
            Avalonia.Data.BindingErrorType.Error
        );
    }
}