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
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PKXIconGen.AvaloniaUI
{
    internal static class Utils
    {
        public static IClassicDesktopStyleApplicationLifetime GetApplicationLifetime()
        {
            return Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime ?? throw new InvalidOperationException();
        }

        public static Window GetMainWindow()
        {
            return GetApplicationLifetime().MainWindow!;
        }

        public static void OpenUrl(string url)
        {
            // For .NETCore 3 and more: https://stackoverflow.com/a/43232486
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (OperatingSystem.IsWindows())
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("open", url);
            }
        }
    }
}
