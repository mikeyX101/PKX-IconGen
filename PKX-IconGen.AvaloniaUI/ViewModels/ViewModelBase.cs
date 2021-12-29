using PKXIconGen.Core.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        #region Misc
        private bool isWindows;
        public bool IsWindows
        {
            get => isWindows;
            private set
            {
                this.RaiseAndSetIfChanged(ref isWindows, value);
            }
        }
        #endregion

        public ViewModelBase()
        {
            isWindows = OperatingSystem.IsWindows();
        }

        private protected static void DoDBQuery(Action<Database> action)
        {
            using Database db = new();
            action(db);
        }

        private protected static T DoDBQuery<T>(Func<Database, T> func)
        {
            using Database db = new();
            return func(db);
        }
    }
}
