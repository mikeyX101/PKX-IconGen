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
using Avalonia.Data.Converters;
using AvaloniaColor = Avalonia.Media.Color;
using PKXIconGen.Core.Data.Blender;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace PKXIconGen.AvaloniaUI.Converters
{
    public class PKXColorAvaloniaColorConverter : IValueConverter
    {
        public static PKXColorAvaloniaColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }


            if (value is Color color && targetType.IsAssignableFrom(typeof(AvaloniaColor)))
            {
                return AvaloniaColor.FromUInt32(color.ToUInt());
            }
            else
            {
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is AvaloniaColor color && targetType.IsAssignableFrom(typeof(Color)))
            {
                return Color.FromRgbInt(color.ToUint32());
            }
            else
            {
                return null;
            }
        }
    }
}
