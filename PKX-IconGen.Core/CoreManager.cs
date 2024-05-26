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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LinqToDB.EntityFrameworkCore;
using PKXIconGen.Core.Logging;
using PKXIconGen.Core.Services;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;

namespace PKXIconGen.Core
{
    public static class CoreManager
    {
        internal const string LoggingAssemblyPropertyName = "LoggingAssembly";
        private const byte MaxLogFiles = 5;

        private static IDisposable? _disposableProperty;
        public static ILogger Logger
        {
            get {
                // Update calling assembly
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                _disposableProperty?.Dispose();
                _disposableProperty = Serilog.Context.LogContext.PushProperty(LoggingAssemblyPropertyName, callingAssembly.GetName().Name);
                
                return NullableLogger ?? throw new ArgumentNullException(nameof(NullableLogger), "CoreManager.Initiate must be called before using the Logger.");
            }
        }
        private static Logger? NullableLogger { get; set; }

        public static Task? DatabaseMigrationTask { get; private set; }
        
        public static bool Initiated { get; private set; }
        public static void Initiate()
        {
            if (Initiated)
            {
                Logger.Warning("Tried to initiate Core again");
                return;
            }

            if (!Directory.Exists(Paths.TempFolder))
            {
                Directory.CreateDirectory(Paths.TempFolder);
            }

            if (!Directory.Exists(Paths.TempBlendFolder))
            {
                Directory.CreateDirectory(Paths.TempBlendFolder);
            }

            if (!Directory.Exists(Paths.LogFolder))
            {
                Directory.CreateDirectory(Paths.LogFolder);
            }
            
            if (!Directory.Exists(Paths.LogFolder))
            {
                Directory.CreateDirectory(Paths.LogFolder);
            }

            Serilog.Formatting.ITextFormatter textFormatter = new LogTemplateFormatter();
            NullableLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Async(config => 
                    config.File(textFormatter, Paths.Log, 
                        buffered: true,
                        rollOnFileSizeLimit: false,
                        flushToDiskInterval: TimeSpan.FromSeconds(15)
                    )
                )     
#if DEBUG
                .WriteTo.Debug(textFormatter)
#endif
                .WriteTo.Console(textFormatter)
                .CreateLogger();
            
            LinqToDBForEFTools.Initialize();
            
            Logger.Information("Starting database migration Task");
            DatabaseMigrationTask = Database.Instance.GetMigrationTask();
            
            Logger.Information("PKX-IconGen Core initiated");
            Initiated = true;
        }

        public static void OnClose()
        {
            Logger.Information("PKX-IconGen Core shutting down gracefully...");
            NameMap.CleanUp();
            Database.OnClose();
            DisposeLogger();
            
            // Copy log as latest.log
            File.Copy(Paths.Log, Paths.LogLatest, true);
            RespectMaxLogs();
        }

        private static void DisposeLogger()
        {
            if (NullableLogger != null)
            {
                ((Logger)Logger).Dispose();
                NullableLogger = null;
            }
        }

        private static void RespectMaxLogs()
        {
            DirectoryInfo info = new DirectoryInfo(Paths.LogFolder);
            FileInfo[] files = info.EnumerateFiles("log*.log").OrderBy(p => p.CreationTime).ToArray();
            if (files.Length <= MaxLogFiles) return;
            
            int nbToDel = MaxLogFiles - files.Length;
            foreach (FileInfo file in files)
            {
                file.Delete();

                if (nbToDel-- <= MaxLogFiles)
                {
                    break;
                }
            }
        }
    }
}
