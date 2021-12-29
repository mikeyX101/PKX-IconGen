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

using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        #region Misc
        private bool isWindows;
        public bool IsWindows
        {
            get => isWindows;
            private set
            {
                this.RaiseAndSetIfChanged(ref isWindows, value);
            }
        }
        #endregion

        public ViewModelBase()
        {
            isWindows = OperatingSystem.IsWindows();
        }

        private protected static void DoDBQuery(Action<Database> action)
        {
            using Database db = new();
            action(db);
        }

        private protected static T DoDBQuery<T>(Func<Database, T> func)
        {
            using Database db = new();
            return func(db);
        }
    }
}
