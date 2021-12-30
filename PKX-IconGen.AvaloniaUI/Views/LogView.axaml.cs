using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PKXIconGen.AvaloniaUI.Views
{
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void LogScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentDelta != Vector.Zero && sender != null)
            {
                ScrollViewer scrollViewer = (ScrollViewer)sender;
                scrollViewer.ScrollToEnd();
            }
        }
    }
}
