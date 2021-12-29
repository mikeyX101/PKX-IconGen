using CliWrap;
using CliWrap.EventStream;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
