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
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using JetBrains.Annotations;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        public static string LogFont => OperatingSystem.IsWindows() ? "Consolas" : "DejaVu Sans Mono";
        
        private readonly StringBuilder logBuilder;
        public string LogText => logBuilder.ToString();

        private const ushort UpdateRate = 250;
        private Task? updatePendingTask;

        private const int initialCapacity = 8192;
        private const int maxCapacity = initialCapacity * 4;
        public LogViewModel()
        {
            logBuilder = new StringBuilder(initialCapacity, maxCapacity);
        }

        [UsedImplicitly]
        public void ClearLog()
        {
            logBuilder.Clear();
            this.RaisePropertyChanged(nameof(LogText));
        }
        public void WriteLine(ReadOnlyMemory<char> line)
        {
            AssureMaxCapacity(line, true);
            logBuilder.Append(line).Append('\n');
            ScheduleUpdate();
        }
        public void Write(ReadOnlyMemory<char> line)
        {
            AssureMaxCapacity(line, false);
            logBuilder.Append(line);
            ScheduleUpdate();
        }

        private void AssureMaxCapacity(ReadOnlyMemory<char> line, bool hasNewLine)
        {
            int lengthToInsert = line.Length + (hasNewLine ? 1 : 0);
            while (logBuilder.Length + lengthToInsert > maxCapacity)
            {
                int lastNewLineIndex = logBuilder.Length - 1;
                for (int i = 0; i < logBuilder.Length; i++)
                {
                    if (logBuilder[i] == '\n')
                    {
                        lastNewLineIndex = i;
                        break;
                    }
                }
                // Remove lines until new line is insertable
                logBuilder.Remove(0, lastNewLineIndex + 1);
            }
        }
        private async void ScheduleUpdate()
        {
            if (updatePendingTask == null)
            {
                updatePendingTask = Task.Run(async () => {
                    await Task.Delay(UpdateRate);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        this.RaisePropertyChanged(nameof(LogText));
                    });
                });
                await updatePendingTask;
                
                updatePendingTask.Dispose();
                updatePendingTask = null;
            }
        }
    }
}
