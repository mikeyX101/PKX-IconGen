using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Logging
{
    // https://github.com/serilog/serilog-sinks-file/issues/137
    internal class LogTemplateFormatter : ITextFormatter
    {
        readonly MessageTemplateTextFormatter
            _withProperties = new("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] [{" + CoreManager.loggingAssemblyPropertyName + "}] {Message:lj}{NewLine}{Exception}{Properties:j}"),
            _withoutProperties = new("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] [{" + CoreManager.loggingAssemblyPropertyName + "}] {Message:lj}{NewLine}{Exception}");

        public void Format(LogEvent logEvent, TextWriter output)
        {
            HashSet<string> tokens = new(logEvent.MessageTemplate.Tokens.OfType<PropertyToken>().Select(p => p.PropertyName));
            tokens.Add(CoreManager.loggingAssemblyPropertyName);

            MessageTemplateTextFormatter formatter = logEvent.Properties.All(p => tokens.Contains(p.Key)) ? _withoutProperties : _withProperties;
            formatter.Format(logEvent, output);
        }
    }
}
