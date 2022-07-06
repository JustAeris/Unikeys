using System;
using System.Windows;

namespace Unikeys;

public partial class CustomMessageBox : Window
{
    public CustomMessageBox(string title, string message, CustomMessageBoxIcons icon)
    {
        InitializeComponent();
        Title = title;
        MessageTextBlock.Text = message;
        switch (icon)
        {
            case CustomMessageBoxIcons.None:
                break;
            case CustomMessageBoxIcons.Error:
                IconSvgViewbox.Source = new Uri("Icons/alert-octogon-black.svg", UriKind.Relative);
                break;
            case CustomMessageBoxIcons.Info:
                IconSvgViewbox.Source = new Uri("Icons/info-black.svg", UriKind.Relative);
                break;
            case CustomMessageBoxIcons.Question:
                IconSvgViewbox.Source = new Uri("Icons/help-black.svg", UriKind.Relative);
                break;
            case CustomMessageBoxIcons.Warning:
                IconSvgViewbox.Source = new Uri("Icons/alert-triangle-black.svg", UriKind.Relative);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Close();
    public enum CustomMessageBoxIcons
    {
        None,
        Error,
        Info,
        Question,
        Warning
    }
}
