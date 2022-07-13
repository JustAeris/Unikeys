using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Unikeys.Gui;

public partial class CustomMessageBox : Window
{
    private readonly Exception? _exception;

    public CustomMessageBox(string title, string message, CustomMessageBoxIcons icon, Exception? exception = null)
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
            case CustomMessageBoxIcons.Success:
                IconSvgViewbox.Source = new Uri("Icons/circle-check-black.svg", UriKind.Relative);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
        }

        if (exception == null) return;
        ReportButton.Visibility = Visibility.Visible;
        _exception = exception;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) => Close();
    public enum CustomMessageBoxIcons
    {
        None,
        Error,
        Info,
        Question,
        Warning,
        Success
    }

    private void ReportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var s = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        s = Path.GetInvalidPathChars().Concat(new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }).Aggregate(s, (current, c) => current.Replace(c, '-'));

        try
        {
            File.WriteAllText(
                string.Join("", s) + " - error.log", _exception?.ToString());
        }
        catch { /* ignore */ }

        ReportButton.IsEnabled = false;
        ReportButton.Content = "Report created";
    }
}
