#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
    Copyright (C) 2021-2022 mikeyX#4697

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
using PKXIconGen.AvaloniaUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public void AboutCommand()
        {
            Assembly coreAssembly = Assembly.Load("PKX-IconGen.Core");
            Assembly uiAssembly = Assembly.GetExecutingAssembly();
            Assembly avaloniaAssembly = Assembly.Load("Avalonia");

            DialogHelper.ShowDialog("/Assets/gen-icon-x512.png", 
@$"PKX-IconGen by mikeyX
Core: {coreAssembly.GetName().Version?.ToString() ?? "Unknown"} 
UI: {uiAssembly.GetName().Version?.ToString() ?? "Unknown"}

Powered by Avalonia {avaloniaAssembly.GetName().Version?.ToString() ?? "Unknown"} ",
                "About");
        }

        public void QuitCommand()
        {
            Utils.GetApplicationLifetime().Shutdown(0);
        }
    }
}
