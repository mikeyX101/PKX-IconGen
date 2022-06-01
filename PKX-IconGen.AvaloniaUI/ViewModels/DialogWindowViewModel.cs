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
    public class DialogWindowViewModel : WindowViewModelBase
    {
        private const uint DefaultHeight = 200;
        public uint Height { get; init; }

        public string DialogText { get; init; }
        public string DialogTitle { get; init; }

        public string? Icon { get; init; }
        public Brush? IconColor { get; init; }
        public bool IconVisible { get; init; }
        
        public string? ImageAsset { get; init; }
        public bool ImageVisible { get; init; }

        public bool OkButtonVisible { get; init; }
        public bool YesNoButtonsVisible { get; init; }

        private DialogWindowViewModel(uint? height, string text, string title, DialogButtons dialogButtons)
        {
            Height = height ?? DefaultHeight;
            DialogText = text;
            DialogTitle = title;

            switch (dialogButtons)
            {
                case DialogButtons.YesNo:
                    YesNoButtonsVisible = true;
                    break;
                
                case DialogButtons.Ok or _:
                    OkButtonVisible = true;
                    break;
            }
        }

        public DialogWindowViewModel(DialogType dialogType, DialogButtons dialogButtons, string text, uint? height = DefaultHeight, string? title = null)
            : this(
                  height,
                  text,
                  title ?? dialogType.GetTitle(),
                  dialogButtons
            )
        {
            Icon = dialogType.GetMaterialDesignIcon();
            IconColor = new SolidColorBrush(Color.FromUInt32(dialogType.GetColor()));
            IconVisible = true;
        }

        public DialogWindowViewModel(string asset, DialogButtons dialogButtons, string text, string title, uint? height = DefaultHeight)
            : this(
                  height,
                  text,
                  title,
                  dialogButtons
            )
        {
            ImageAsset = asset;
            ImageVisible = true;
        }
    }
}
