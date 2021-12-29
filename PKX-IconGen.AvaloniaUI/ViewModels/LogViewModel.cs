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
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private string logText;
        public string LogText
        {
            get => logText;
            set
            {
                this.RaiseAndSetIfChanged(ref logText, value);
            }
        }

        public string LogFont 
        {
            get => OperatingSystem.IsWindows() ? "Consolas" : "DejaVu Sans Mono";
        }

        public LogViewModel()
        {
            logText = "";
        }

        public void ClearLog()
        {
            LogText = "";
        }

        public void Write(string line)
        {
            LogText += line + "\n";
        }
    }
}
