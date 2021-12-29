using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private string logText;
        public string LogText
        {
            get => logText;
            set
            {
                this.RaiseAndSetIfChanged(ref logText, value);
            }
        }

        public string LogFont 
        {
            get => OperatingSystem.IsWindows() ? "Consolas" : "DejaVu Sans Mono";
        }

        public LogViewModel()
        {
            logText = "";
        }

        public void ClearLog()
        {
            LogText = "";
        }

        public void Write(string line)
        {
            LogText += line + "\n";
        }
    }
}
