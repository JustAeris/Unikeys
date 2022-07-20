using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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

        // Set the version number in the title bar
        Title = $"Unikeys {Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
