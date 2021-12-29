using Avalonia;
using PKXIconGen.AvaloniaUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public void AboutCommand()
        {
            Assembly coreAssembly = Assembly.Load("PKX-IconGen.Core");
            Assembly uiAssembly = Assembly.GetExecutingAssembly();
            Assembly avaloniaAssembly = Assembly.Load("Avalonia");

            DialogHelper.ShowDialog("/Assets/gen-icon-x512.png", 
@$"PKX-IconGen by mikeyX
Core: {coreAssembly.GetName().Version?.ToString() ?? "Unknown"} 
UI: {uiAssembly.GetName().Version?.ToString() ?? "Unknown"}

Powered by Avalonia {avaloniaAssembly.GetName().Version?.ToString() ?? "Unknown"} ",
                "About");
        }

        public void QuitCommand()
        {
            Utils.GetApplicationLifetime().Shutdown(0);
        }
    }
}
