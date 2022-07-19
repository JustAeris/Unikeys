using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ModernWpf;

namespace Unikeys.Gui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public partial class MainWindow
{
    /// <summary>
    /// Window constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        // Dark theme will come soon
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

        // Set the version number in the title bar
        Title = $"Unikeys v{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
