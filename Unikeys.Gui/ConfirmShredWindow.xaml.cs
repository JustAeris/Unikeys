using System.Threading.Tasks;
using System.Windows;

namespace Unikeys.Gui;

public partial class ConfirmShredWindow
{
    public bool Confirmed { get; private set; }

    public ConfirmShredWindow()
    {
        Confirmed = false;
        InitializeComponent();
        ButtonTimer();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
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
        ConfirmButton.IsEnabled = true;
    }

    private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
