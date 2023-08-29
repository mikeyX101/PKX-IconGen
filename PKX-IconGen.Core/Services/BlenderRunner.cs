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

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.EventStream;
using PKXIconGen.Core.Data;

namespace PKXIconGen.Core.Services
{
    public interface IBlenderRunner
    {
        Task RunAsync(CancellationToken? cancellationToken = null);

        delegate void OutDel(ReadOnlyMemory<char> output);
        event OutDel OnOutput;

        delegate void FinishDel(PokemonRenderData? prd);
        event FinishDel OnFinish;
    }

    internal class BlenderRunner : IBlenderRunner
    {
        internal static class BlenderRunners
        {
            internal static IBlenderRunner GetRenderRunner(IBlenderRunnerInfo blenderRunnerInfo, RenderJob job) => 
                new BlenderRunner(blenderRunnerInfo, job.Data.TemplateName, new string[]
                {
                    "--background",
                    "--python", Paths.Render
                }, JsonIO.ToJsonString(job));

            internal static IBlenderRunner GetModifyDataRunner(IBlenderRunnerInfo blenderRunnerInfo, PokemonRenderData prd) => 
                new BlenderRunner(blenderRunnerInfo, prd.TemplateName, new string[]
                {
                    "--python", Paths.ModifyData
                }, JsonIO.ToJsonString(prd), prd.Output);
        }

        private const string LogTemplate = "[CLIWrap] -> [{ExecutableName}] {Output}";

        private string TemplateName { get; init; }
        private bool LogBlender { get; init; }
        private string BlenderPath { get; init; }
        private string AssetsPath { get; init; }
        private string[] Arguments { get; init; }
        private string OptionalArguments { get; init; }
        private byte[] Input { get; init; }
        private string? ExpectedJsonName { get; init; }
        private string ExecutableName { get; init; }

        private BlenderRunner(IBlenderRunnerInfo blenderRunnerInfo, string templateName, string[] arguments, string input, string? expectedJsonName = null)
        {
            TemplateName = templateName;
            LogBlender = blenderRunnerInfo.LogBlender;
            BlenderPath = blenderRunnerInfo.Path;
            AssetsPath = blenderRunnerInfo.AssetsPath;
            Arguments = arguments;
            OptionalArguments = blenderRunnerInfo.OptionalArguments;
            Input = Encoding.UTF8.GetBytes(input);
            ExpectedJsonName = expectedJsonName;
            ExecutableName = Path.GetFileName(blenderRunnerInfo.Path);

            if (LogBlender)
            {
                OnOutput += Log;
            }
        }

        #region Event
        private static object ObjectLock => new();
        
        public event IBlenderRunner.OutDel? OnOutput;
        event IBlenderRunner.OutDel IBlenderRunner.OnOutput
        {
            add
            {
                lock (ObjectLock)
                {
                    OnOutput += value;
                }
            }

            remove
            {
                lock (ObjectLock)
                {
                    OnOutput -= value;
                }
            }
        }

        public event IBlenderRunner.FinishDel? OnFinish;
        event IBlenderRunner.FinishDel IBlenderRunner.OnFinish
        {
            add
            {
                lock (ObjectLock)
                {
                    OnFinish += value;
                }
            }

            remove
            {
                lock (ObjectLock)
                {
                    OnFinish -= value;
                }
            }
        }
        #endregion

        private void Log(ReadOnlyMemory<char> s)
        {
            CoreManager.Logger.Information(LogTemplate, ExecutableName, s);
        }

        public async Task RunAsync(CancellationToken? cancellationToken = null)
        {
            CancellationToken token = cancellationToken ?? CancellationToken.None;

            if (!Directory.Exists(Paths.BlenderLogsFolder))
            {
                Directory.CreateDirectory(Paths.BlenderLogsFolder);
            }

            Command cmd = Cli.Wrap(BlenderPath)
                .WithWorkingDirectory(Paths.PythonFolder)
                .WithArguments(args =>
                {
                    if (LogBlender)
                    {
                        args
                            .Add("--log").Add("*")
                            .Add("--log-file").Add(Paths.BlenderLog);
                    }

                    args
                        .Add("--debug-python")
                        .Add("--enable-autoexec")
                        .Add("--python-exit-code").Add("200")
                        .Add(Paths.GetTemplateCopy(TemplateName))
                        .Add(Arguments);

                    if (!string.IsNullOrWhiteSpace(OptionalArguments))
                    {
                        args.Add(OptionalArguments);
                    }

                    args
                        .Add("--") // Python Script args
                        .Add("--assets-path").Add(AssetsPath);
                });
            
            cmd = cmd.WithStandardInputPipe(PipeSource.FromBytes(Input));
            PokemonRenderData? prd = null;
            
            try
            {
                await foreach (var cmdEvent in cmd.ListenAsync(token))
                {
                    switch (cmdEvent)
                    {
                        case StartedCommandEvent started:
                            OnOutput?.Invoke($"Process started; ID: {started.ProcessId}; Arguments: {cmd.Arguments}".AsMemory());
                            break;

                        case StandardOutputCommandEvent stdOut:
                            OnOutput?.Invoke($"Out> {stdOut.Text}".AsMemory());
                            break;
                        case StandardErrorCommandEvent stdErr:
                            OnOutput?.Invoke($"Err> {stdErr.Text}".AsMemory());
                            break;

                        case ExitedCommandEvent exited:
                            if (ExpectedJsonName is not null)
                            {
                                string jsonPath = Path.Combine(Paths.TempFolder, ExpectedJsonName + ".json");
                                if (File.Exists(jsonPath))
                                {
                                    OnOutput?.Invoke("Reading output Json...".AsMemory());
                                    try
                                    {
                                        prd = await JsonIO.ImportAsync<PokemonRenderData>(jsonPath);
                                    }
                                    finally
                                    {
                                        File.Delete(jsonPath);
                                    }
                                }
                            }
                            
                            OnOutput?.Invoke($"Process exited; Code: {exited.ExitCode}".AsMemory());
                            OnOutput?.Invoke("Operation has finished.".AsMemory());
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                OnOutput?.Invoke("Operation was canceled.".AsMemory());
            }
            catch (Win32Exception e)
            {
                OnOutput?.Invoke(("EXCEPTION> An error occured :\n" + e.StackTrace).AsMemory());
                OnOutput?.Invoke("Check Blender Path, it is probably invalid.".AsMemory());
                
                throw;
            }
            catch (CliWrap.Exceptions.CommandExecutionException e)
            {
                if (e.ExitCode == 200)
                {
                    OnOutput?.Invoke(("EXCEPTION> An error occured in a python script, see logs for further details :\n" + e.StackTrace).AsMemory());
                }
                else
                {
                    OnOutput?.Invoke(("EXCEPTION> An error occured in Blender, see logs for further details :\n Exit code: " + e.ExitCode + "\n" + e.StackTrace).AsMemory());
                }

                throw;
            }
            catch (Exception e)
            {
                OnOutput?.Invoke(("EXCEPTION> An unknown error occured :\n" + e.StackTrace).AsMemory());
                
                throw;
            }
            finally
            {
                await Utils.CleanBlend1Files();
                OnFinish?.Invoke(prd);
            }
        }
    }
}
