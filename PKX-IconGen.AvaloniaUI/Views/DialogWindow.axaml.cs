using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PKXIconGen.AvaloniaUI.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
