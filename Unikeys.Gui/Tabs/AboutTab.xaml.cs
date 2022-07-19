using System.Reflection;

namespace Unikeys.Gui.Tabs;

public partial class AboutTab
{
    public AboutTab()
    {
        InitializeComponent();

        VersionLabel.Content += Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }
}
