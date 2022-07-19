using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Unikeys.Gui;

public partial class UniqueKeyDisplayWindow
{
    private bool _cancelClosing = true;

    public UniqueKeyDisplayWindow(string keyToDisplay)
    {
        InitializeComponent();
        UniqueKeyBox.Text = keyToDisplay;
        UniqueKeyBox.Focus();
        ButtonTimer();
    }

    private void UniqueKeyDisplayWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = _cancelClosing;
    }

    private async void ButtonTimer()
    {
        var i = 5;
        while (i > 0)
        {
            ProceedIn.Text = "Proceed in " + i + " seconds...";
            await Task.Delay(1000);
            i--;
        }
        ProceedIn.Text = "Proceed";
        CloseButton.IsEnabled = true;
        _cancelClosing = false;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
