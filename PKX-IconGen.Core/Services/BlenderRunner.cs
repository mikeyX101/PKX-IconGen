#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
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

using CliWrap;
using CliWrap.EventStream;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace PKXIconGen.Core.Services
{
    public class BlenderRunner
    {
        private const string LogTemplate = "[CLIWrap] -> [{ExecutableName}] {Output}";

        public string BlenderPath { get; private set; }
        private string ExecutableName { get; }
        
        public BlenderRunner(string blenderPath)
        {
            BlenderPath = blenderPath;
            ExecutableName = Path.GetFileName(blenderPath);

            OnOutput += Log;
            OnFinish += () => { };
        }

        public delegate void OutDel(string output);
        public event OutDel OnOutput;

        public delegate void FinishDel();
        public event FinishDel OnFinish;

        private void Log(string s)
        {
            CoreManager.Logger.Information(LogTemplate, ExecutableName, s);
        }

        public async void RunRenderAsync(CancellationToken? cancellationToken = null)
        {
            CancellationToken token = cancellationToken ?? CancellationToken.None;

            Command cmd = Cli.Wrap(BlenderPath)
                .WithArguments(new string[]
                {
                    "--background"
                });

            try
            {
                await foreach (var cmdEvent in cmd.ListenAsync(token))
                {
                    switch (cmdEvent)
                    {
                        case StartedCommandEvent started:
                            OnOutput($"Process started; ID: {started.ProcessId}");
                            break;

                        case StandardOutputCommandEvent stdOut:
                            OnOutput($"Out> {stdOut.Text}");
                            break;
                        case StandardErrorCommandEvent stdErr:
                            OnOutput($"Err> {stdErr.Text}");
                            break;

                        case ExitedCommandEvent exited:
                            OnOutput($"Process exited; Code: {exited.ExitCode}");
                            OnOutput("Render has finished.");
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                OnOutput("Render was canceled.");
            }
            catch (Win32Exception e)
            {
                OnOutput("EXCEPTION> An error occured : " + e.Message);
                OnOutput("Check Blender Path, it is probably invalid.");
            }
            catch (Exception e)
            {
                OnOutput("EXCEPTION> An unknown error occured : " + e.Message);
            }
            finally
            {
                OnFinish();
            }
        }
    }
}
