﻿using PKXIconGen.Core.Logging;
using PKXIconGen.Core.Services;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        private static bool Initiated = false;
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
                        rollOnFileSizeLimit: false
                    )
                )     
#if DEBUG
                .WriteTo.Debug(textFormatter)
                .WriteTo.Console(textFormatter)
#endif
                .CreateLogger();


            using Database db = new();
            db.RunMigrations();

            Initiated = true;
            Logger.Information("PKX-IconGen Core initiated!");
        }

        public static void OnApplicationEnd(object? sender, EventArgs eventArgs)
        {
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
