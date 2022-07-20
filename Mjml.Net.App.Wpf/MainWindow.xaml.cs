using System.Windows;

namespace Mjml.Net.Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = Wpf.Ui.Appearance.Background.Apply(this, Wpf.Ui.Appearance.BackgroundType.Mica);
        }
    }
}
