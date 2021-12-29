using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using PKXIconGen.AvaloniaUI.Models.Dialog;
using System.Windows.Input;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class DialogWindowViewModel : ViewModelBase
    {
        public string DialogText { get; set; }
        public string DialogTitle { get; set; }

        public string? Icon { get; set; }
        public Brush? IconColor { get; set; }
        public bool IconVisible { get; set; } = false;
        
        public string? ImageAsset { get; set; }
        public bool ImageVisible { get; set; } = false;

        private DialogWindowViewModel(string text, string title)
        {
            DialogText = text;
            DialogTitle = title;
        }

        public DialogWindowViewModel(DialogType dialogType, string text, string? title = null)
            : this(
                  text,
                  title ?? GetTitle(dialogType)
            )
        {
            Icon = "fas " + GetFontAwesomeIcon(dialogType);
            IconColor = new SolidColorBrush(Color.FromUInt32(GetIconColor(dialogType)));
            IconVisible = true;
        }

        public DialogWindowViewModel(string asset, string text, string title)
            : this(
                  text,
                  title
            )
        {
            ImageAsset = asset;
            ImageVisible = true;
        }

        private static string GetFontAwesomeIcon(DialogType dialogType)
        {
            return dialogType switch
            {
                DialogType.Warning      => "fa-exclamation-triangle",
                DialogType.Error        => "fa-exclamation-circle",
                DialogType.Text or _    => ""
            };
        }

        private static uint GetIconColor(DialogType dialogType)
        {
            return dialogType switch
            {
                DialogType.Warning      => 0xffffc107,
                DialogType.Error        => 0xffff0000,
                DialogType.Text or _    => 0xffffffff
            };
        }

        private static string GetTitle(DialogType dialogType)
        {
            return dialogType switch
            {
                DialogType.Warning      => "Warning",
                DialogType.Error        => "Error",
                DialogType.Text or _    => ""
            };
        }
    }
}
