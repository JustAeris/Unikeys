using System.Windows;
using ModernWpf;

namespace Unikeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Dark theme will come soon
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

            VersionLabel.Content += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }
    }
}
