﻿#region License
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
using CliWrap.Builders;
using CliWrap.EventStream;
using PKXIconGen.Core.Data;

namespace PKXIconGen.Core.Services;

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
            new BlenderRunner(blenderRunnerInfo, job.Data.TemplateName, [
                "--background",
                "--python", Paths.Render
            ], JsonIO.ToJsonString(job));

        internal static IBlenderRunner GetModifyDataRunner(IBlenderRunnerInfo blenderRunnerInfo,
            PokemonRenderData prd) =>
            new BlenderRunner(blenderRunnerInfo, prd.TemplateName, [
                "--python", Paths.ModifyData
            ], JsonIO.ToJsonString(prd), prd.Output);
    }

    private const string LogTemplate = "[CLIWrap] -> [{ExecutableName}] {Output}";

    private string TemplateName { get; }
    private bool LogBlender { get; }
    private string BlenderPath { get; }
    private string AssetsPath { get; }
    private bool ShowXDCutout { get; }
    private string[] Arguments { get; }
    private string OptionalArguments { get; }
    private byte[] Input { get; }
    private string? ExpectedJsonName { get; }
    private string ExecutableName { get; }
    
    private PokemonRenderData? ResultPRD { get; set; }

    private BlenderRunner(IBlenderRunnerInfo blenderRunnerInfo, string templateName, string[] arguments, string input, string? expectedJsonName = null)
    {
        TemplateName = templateName;
        LogBlender = blenderRunnerInfo.LogBlender;
        BlenderPath = blenderRunnerInfo.BlenderPath;
        AssetsPath = blenderRunnerInfo.AssetsPath;
        ShowXDCutout = blenderRunnerInfo.ShowXDCutout;
        Arguments = arguments;
        OptionalArguments = blenderRunnerInfo.BlenderOptionalArguments;
        Input = Encoding.UTF8.GetBytes(input);
        ExpectedJsonName = expectedJsonName;
        ExecutableName = Path.GetFileName(blenderRunnerInfo.BlenderPath);

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
        PKXCore.Logger.Information(LogTemplate, ExecutableName, s);
    }
    
    private void BuildArguments(ArgumentsBuilder args)
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
            args.Add(OptionalArguments.Split(' '));
        }

        args
            .Add("--") // Python Script args
            .Add("--assets-path").Add(AssetsPath);

        if (ShowXDCutout)
        {
            args.Add("--xd-cutout");
        }
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
            .WithArguments(BuildArguments);
            
        cmd = cmd.WithStandardInputPipe(PipeSource.FromBytes(Input));
            
        try
        {
            await foreach (CommandEvent cmdEvent in cmd.ListenAsync(token))
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
                        await OnProcessExitAsync(exited);
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
            OnFinish?.Invoke(ResultPRD);
        }
    }

    private async Task OnProcessExitAsync(ExitedCommandEvent e)
    {
        if (ExpectedJsonName is not null)
        {
            string jsonPath = Path.Combine(Paths.TempFolder, ExpectedJsonName + ".json");
            if (File.Exists(jsonPath))
            {
                OnOutput?.Invoke("Reading output Json...".AsMemory());
                try
                {
                    ResultPRD = await JsonIO.ImportAsync<PokemonRenderData>(jsonPath);
                }
                finally
                {
                    File.Delete(jsonPath);
                }
            }
        }
                            
        OnOutput?.Invoke($"Process exited; Code: {e.ExitCode}".AsMemory());
        OnOutput?.Invoke("Operation has finished.".AsMemory());
    }
}