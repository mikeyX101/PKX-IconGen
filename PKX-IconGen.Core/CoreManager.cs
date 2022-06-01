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

using PKXIconGen.Core.Logging;
using PKXIconGen.Core.Services;
using Serilog;
using Serilog.Exceptions;
using System;
using System.IO;
using System.Reflection;

namespace PKXIconGen.Core
{
    public static class CoreManager
    {
        internal const string LoggingAssemblyPropertyName = "LoggingAssembly";

        private static Assembly? _lastLoggingAssembly;
        private static IDisposable? _disposableProperty;
        public static ILogger Logger
        {
            get {
                // Update calling assembly
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                if (callingAssembly != _lastLoggingAssembly)
                {
                    _disposableProperty?.Dispose();
                    _disposableProperty = Serilog.Context.LogContext.PushProperty(LoggingAssemblyPropertyName, callingAssembly.GetName().Name);
                    _lastLoggingAssembly = callingAssembly;
                }
                
                return NullableLogger ?? throw new ArgumentNullException(nameof(NullableLogger), "CoreManager.Initiate must be called before using the Logger.");
            }
        }
        private static ILogger? NullableLogger { get; set; }

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

            try
            {
                Database db = Database.Instance;
                db.RunMigrations();

                Initiated = true;
                Logger.Information("PKX-IconGen Core initiated");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An exception occured while migrating the database");
            }
        }

        public static void OnClose()
        {
            Logger.Information("PKX-IconGen Core shutting down gracefully...");
            Database.OnClose();
            DisposeLogger();
            
            // Copy log as latest.log
            File.Copy(Paths.Log, Paths.LogLatest, true);
        }
        private static void DisposeLogger()
        {
            if (NullableLogger != null)
            {
                ((Serilog.Core.Logger)Logger).Dispose();
                NullableLogger = null;
            }
        }
    }
}
