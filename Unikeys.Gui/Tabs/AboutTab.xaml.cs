using System.Reflection;
using System.Windows.Controls;

namespace Unikeys.Gui.Tabs;

public partial class AboutTab : UserControl
{
    public AboutTab()
    {
        InitializeComponent();

        VersionLabel.Content += Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }
}
