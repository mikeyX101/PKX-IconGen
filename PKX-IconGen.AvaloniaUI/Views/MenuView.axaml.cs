using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PKXIconGen.AvaloniaUI.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
