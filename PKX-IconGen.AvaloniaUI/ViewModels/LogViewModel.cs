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

using ReactiveUI;
using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private readonly StringBuilder logBuilder;
        public string LogText => logBuilder.ToString();

        private const ushort UpdateRate = 1000;
        private Task? updatePendingTask;

        public static string LogFont => OperatingSystem.IsWindows() ? "Consolas" : "DejaVu Sans Mono";

        public LogViewModel()
        {
            logBuilder = new StringBuilder(1024);
        }

        [UsedImplicitly]
        public void ClearLog()
        {
            logBuilder.Clear();
            this.RaisePropertyChanged(nameof(LogText));
        }
        private const char NewLine = '\n';
        public void WriteLine(ReadOnlyMemory<char> line)
        {
            logBuilder.Append(line).Append(NewLine);
            ScheduleUpdate();
        }

        private async void ScheduleUpdate()
        {
            if (updatePendingTask == null)
            {
                updatePendingTask = Task.Run(async () => {
                    await Task.Delay(UpdateRate);
                    this.RaisePropertyChanged(nameof(LogText));
                });
                await updatePendingTask;
                updatePendingTask = null;
            }
        }
    }
}
