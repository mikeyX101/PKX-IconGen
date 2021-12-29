using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.Converters
{
    public class StringFontFamilyConverter : IValueConverter
    {
        public static StringFontFamilyConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string fontName && targetType.IsAssignableFrom(typeof(FontFamily)))
            {
                return new FontFamily(fontName);
            }
            else
            {
                return null;
            }
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
