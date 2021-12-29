using CliWrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Services
{
    public class BlenderCheckResult
    {
        public bool IsBlender { get; private set; }

        public bool IsValidVersion { get; private set; }

        public string Version { get; private set; }

        public BlenderCheckResult(bool isBlender, bool isValidVersion, string version)
        {
            IsBlender = isBlender;
            IsValidVersion = isValidVersion;
            Version = version;
        }
    }

    public class BlenderVersionChecker
    {
        private const string SupportedBlenderVersion = "2.83";

        public BlenderCheckResult? CheckExecutable(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (!OperatingSystem.IsWindows())
                {
                    return new(true, true, "Unknown");
                }
                else if (File.Exists(path))
                {
                    // Windows has metadata we can use.
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                    string? name = fileVersionInfo.ProductName;
                    string? version = fileVersionInfo.ProductVersion;

                    if (name != null && version != null)
                    {
                        bool isBlender = name.Contains("Blender");
                        bool isValidVersion = version.Contains(SupportedBlenderVersion);
                        return new(isBlender, isValidVersion, version);    
                    }
                }
            }
            return null;
        }
    }
}
