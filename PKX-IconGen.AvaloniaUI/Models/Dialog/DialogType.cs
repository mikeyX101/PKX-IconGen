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

namespace PKXIconGen.AvaloniaUI.Models.Dialog;

public enum DialogType
{
    Info    = 0,
    Warning = 1,
    Error   = 2
}

public static class DialogTypeExtensions
{
    public static string GetMaterialDesignIcon(this DialogType dialogType)
    {
        return dialogType switch
        {
            DialogType.Warning      => "mdi-alert",
            DialogType.Error        => "mdi-close-circle",
            DialogType.Info or _    => "mdi-information"
        };
    }

    public static uint GetColor(this DialogType dialogType)
    {
        return dialogType switch
        {
            DialogType.Warning      => 0xffffc107,
            DialogType.Error        => 0xffff0000,
            DialogType.Info or _    => 0xffffffff
        };
    }
    public static string GetTitle(this DialogType dialogType)
    {
        return dialogType switch
        {
            DialogType.Warning      => "Warning",
            DialogType.Error        => "Error",
            DialogType.Info or _    => "Information"
        };
    }
}