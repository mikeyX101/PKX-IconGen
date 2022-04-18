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
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace PKXIconGen.AvaloniaUI.Converters
{
    public class EnumerableSemiColonStringConverter : IValueConverter
    {
        public static EnumerableSemiColonStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IEnumerable items && targetType.IsAssignableFrom(typeof(string)))
            {
                IEnumerable<object> objectItems = items.OfType<object>();

                if (!objectItems.Any())
                {
                    return "";
                }
                else
                {
                    return objectItems.Select(o => o.ToString()).Aggregate((s1, s2) => $"{s1};{s2};");
                }
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

            if (value is string s && targetType.IsAssignableFrom(typeof(IEnumerable)))
            {
                return s.Split(';', options: StringSplitOptions.None);
            }
            else
            {
                return null;
            }
        }
    }
}
