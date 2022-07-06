namespace Unikeys;

public partial class UniqueKeyDisplayWindow
{
    public UniqueKeyDisplayWindow(string keyToDisplay)
    {
        InitializeComponent();
        UniqueKeyBox.Text = keyToDisplay;
    }
}
