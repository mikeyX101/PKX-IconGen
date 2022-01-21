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

using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        #region OS
        public bool IsWindows { get; init; }
        public bool IsMacOS { get; init; }
        public bool IsLinux { get; init; }
        #endregion

        public ViewModelBase()
        {
            IsWindows = OperatingSystem.IsWindows();
            IsMacOS = OperatingSystem.IsMacOS();
            IsLinux = OperatingSystem.IsLinux();
        }

        private protected static void DoDBQuery(Action<Database> action)
        {
            action(Database.Instance);
        }

        private protected static T DoDBQuery<T>(Func<Database, T> func)
        {
            return func(Database.Instance);
        }

        private protected async static Task DoDBQueryAsync(Func<Database, Task> func)
        {
            await func(Database.Instance);
        }

        private protected async static Task<T> DoDBQueryAsync<T>(Func<Database, Task<T>> func)
        {
            return await func(Database.Instance);
        }
    }

    public class WindowViewModelBase : ViewModelBase
    {
        public WindowViewModelBase() : base()
        {
            
        }
    }
}
