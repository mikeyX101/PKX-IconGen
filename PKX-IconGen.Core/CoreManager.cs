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
        internal const string loggingAssemblyPropertyName = "LoggingAssembly";

        private static Assembly? lastLoggingAssembly = null;
        private static IDisposable? disposableProperty;
        public static ILogger Logger
        {
            get {
                // Update calling assembly
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                if (callingAssembly != lastLoggingAssembly)
                {
                    if (disposableProperty != null)
                    {
                        disposableProperty.Dispose();
                    }
                    disposableProperty = Serilog.Context.LogContext.PushProperty(loggingAssemblyPropertyName, callingAssembly.GetName().Name);
                    lastLoggingAssembly = callingAssembly;
                }
                
                return NullableLogger ?? throw new ArgumentNullException(nameof(NullableLogger), "CoreManager.Initiate must be called before using the Logger.");
            }
        }
        private static ILogger? NullableLogger { get; set; }

        public static bool Initiated { get; private set; } = false;
        public static void Initiate()
        {
            if (Initiated)
            {
                Logger.Warning("Tried to reinitiate Core.");
                return;
            }

            string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string logFilePath = Path.Combine(logDirectoryPath, $"log-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }

            Serilog.Formatting.ITextFormatter textFormatter = new LogTemplateFormatter();
            NullableLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Async(config => 
                    config.File(textFormatter, logFilePath, 
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
                using Database db = new();
                db.RunMigrations();

                Initiated = true;
                Logger.Information("PKX-IconGen Core initiated!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An exception occured while migrating the database.");
            }
        }

        public static void OnApplicationEnd(object? sender, EventArgs eventArgs)
        {
            Logger.Information("PKX-IconGen Core shuting down gracefully...");
            DisposeLogger();
        }
        public static void DisposeLogger()
        {
            if (NullableLogger != null)
            {
                ((Serilog.Core.Logger)Logger).Dispose();
                NullableLogger = null;
            }
        }
    }
}
