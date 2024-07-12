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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;

namespace PKXIconGen.Core.Logging;

// https://github.com/serilog/serilog-sinks-file/issues/137
internal class LogTemplateFormatter : ITextFormatter
{
    private readonly MessageTemplateTextFormatter
        withProperties = new("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] [{" + PKXCore.LoggingAssemblyPropertyName + "}] {Message:lj}{NewLine}{Exception}{Properties:j}"),
        withoutProperties = new("{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u4}] [{" + PKXCore.LoggingAssemblyPropertyName + "}] {Message:lj}{NewLine}{Exception}");

    public void Format(LogEvent logEvent, TextWriter output)
    {
        HashSet<string> tokens =
        [
            ..logEvent.MessageTemplate.Tokens.OfType<PropertyToken>().Select(p => p.PropertyName),
            PKXCore.LoggingAssemblyPropertyName
        ];

        MessageTemplateTextFormatter formatter = logEvent.Properties.All(p => tokens.Contains(p.Key)) ? withoutProperties : withProperties;
        formatter.Format(logEvent, output);
    }
}