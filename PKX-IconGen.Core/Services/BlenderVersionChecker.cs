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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Exceptions;

namespace PKXIconGen.Core.Services;

public readonly struct BlenderCheckResult(bool isBlender, bool isValidVersion, string version)
{
    public bool IsBlender { get; } = isBlender;

    public bool IsValidVersion { get; } = isValidVersion;

    public string Version { get; } = version;
}

public static class BlenderVersionChecker
{
    private static readonly int[] MinimumBlenderVersion = [4, 5, 0];
    private const float MinimumBlenderVersionFloat = 4.50f;

    public static async Task<BlenderCheckResult?> CheckExecutable(string? path, CancellationToken? cancellationToken = null)
    {
        CancellationToken token = cancellationToken ?? CancellationToken.None;
        
        string? executable = path;
        if (string.IsNullOrWhiteSpace(executable)) 
            return null;

        if (!File.Exists(executable))
        {
            executable = Utils.GetExeFromEnvPath(executable);
            if (executable == null)
            {
                return null;
            }
        }
        
        if (!OperatingSystem.IsWindows())
        {
            return await GetBlenderVersionFromParam(executable, token);
        }
        
        // Windows has metadata we can use.
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(executable);
        string? name = fileVersionInfo.ProductName;
        string? version = fileVersionInfo.FileVersion;

        if (name != null && version != null)
        {
            bool isBlender = name.Equals("Blender");
            bool isValidVersion = VerifyVersionString(version);
            return new BlenderCheckResult(isBlender, isValidVersion, version);    
        }
        return null;
    }

    private static async Task<BlenderCheckResult?> GetBlenderVersionFromParam(string executable, CancellationToken token)
    {
        using CancellationTokenSource forcefulCts = new();
        using CancellationTokenSource gracefulCts = new();

        await using Stream output = new MemoryStream(2048);
        using StreamReader reader = new(output, Encoding.UTF8, true);
        
        Command cmd = Cli.Wrap(executable)
            .WithArguments("-v")
            .WithStandardOutputPipe(PipeTarget.ToStream(output));

        try
        {
            forcefulCts.CancelAfter(TimeSpan.FromSeconds(3));
            gracefulCts.CancelAfter(TimeSpan.FromSeconds(1));
            CommandResult result = await cmd.ExecuteAsync(forcefulCts.Token, gracefulCts.Token);

            if (result.IsSuccess && !token.IsCancellationRequested)
            {
                reader.BaseStream.Position = 0;
                string? blenderVer = await reader.ReadLineAsync(token);
                bool isBlender = blenderVer?.StartsWith("Blender ") ?? false;
                bool isValidVersion = false;
                string versionStr = "";
                if (blenderVer is { Length: <= 24 } && isBlender)
                {
                    versionStr = blenderVer.Remove(0, 8).Replace("LTS", "").Trim();
                    isValidVersion = VerifyVersionString(versionStr);
                }

                return new BlenderCheckResult(isBlender, isValidVersion, versionStr);
            }
        }
        catch (CommandExecutionException e)
        {
            PKXCore.Logger.Warning(e, "Executable exited with error code {Code}", e.ExitCode);
        }

        return null;
    }
    
    // XX.XX.XX
    private static bool VerifyVersionString(string verString)
    {
        bool success = false;
        int[] version = new int[3];
        
        string[] components = verString.Split('.');
        if (components.Length == 3)
        {
            success = int.TryParse(components[0], out version[0]) && 
                      int.TryParse(components[1], out version[1]) && 
                      int.TryParse(components[2], out version[2]);

            if (success)
            {
                success = 
                    version[0] >= MinimumBlenderVersion[0] &&
                    version[1] >= MinimumBlenderVersion[1] &&
                    version[2] >= MinimumBlenderVersion[2];
            }
        }
            
        return success;
    }
}