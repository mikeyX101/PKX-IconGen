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

using Avalonia.Media;
using PKXIconGen.AvaloniaUI.Models.Dialog;

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
