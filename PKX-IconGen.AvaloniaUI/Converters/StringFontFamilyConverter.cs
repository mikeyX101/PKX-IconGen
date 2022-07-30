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

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PKXIconGen.AvaloniaUI.Converters
{
    public class StringFontFamilyConverter : IValueConverter
    {
        public static readonly StringFontFamilyConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string fontName && targetType.IsAssignableFrom(typeof(FontFamily)))
            {
                return new FontFamily(fontName);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(
                new NotSupportedException("Converting FontFamily back to the original value is not supported."),
                Avalonia.Data.BindingErrorType.Error
            );
        }
    }
}
