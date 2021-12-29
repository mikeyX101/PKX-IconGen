using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// https://docs.avaloniaui.net/docs/controls/image
namespace PKXIconGen.AvaloniaUI.Converters
{
    /// <summary>
    /// <para>
    /// Converts a string path to a bitmap asset.
    /// </para>
    /// <para>
    /// The asset must be in the same assembly as the program. If it isn't,
    /// specify "avares://<assemblynamehere>/" in front of the path to the asset.
    /// </para>
    /// </summary>
    public class BitmapAssetValueConverter : IValueConverter
    {
        public static BitmapAssetValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is string rawUri && targetType.IsAssignableFrom(typeof(Bitmap)))
            {
                Uri? uri = null;

                // Allow for assembly overrides
                if (rawUri.StartsWith("avares://"))
                {
                    uri = new Uri(rawUri);
                }
                else
                {
                    Assembly? assembly = Assembly.GetEntryAssembly();
                    if (assembly != null)
                    {
                        string? assemblyName = assembly.GetName().Name;
                        if (assemblyName != null)
                        {
                            uri = new Uri($"avares://{assemblyName}{rawUri}");
                        }
                    }
                }

                if (uri != null) 
                { 
                    IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>() ?? throw new InvalidOperationException("Asset Loader was null.");
                    Stream assetStream = assets.Open(uri);
                    return new Bitmap(assetStream);
                }
            }

            return new Avalonia.Data.BindingNotification(
                new InvalidDataException("Invalid data while converting to Bitmap object."), 
                Avalonia.Data.BindingErrorType.Error
            );
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(
                new NotSupportedException("Converting Bitmap back to the original value is not supported."),
                Avalonia.Data.BindingErrorType.Error
            );
        }
    }
}
