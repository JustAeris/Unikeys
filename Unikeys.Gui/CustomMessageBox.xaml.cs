using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using SharpVectors.Converters;

namespace Unikeys.Gui;

public partial class CustomMessageBox
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
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "AlertOctagonIcon");
                break;
            case CustomMessageBoxIcons.Info:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "InfoIcon");
                break;
            case CustomMessageBoxIcons.Question:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "HelpIcon");
                break;
            case CustomMessageBoxIcons.Warning:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "AlertTriangleIcon");
                break;
            case CustomMessageBoxIcons.Success:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "CircleCheckIcon");
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
