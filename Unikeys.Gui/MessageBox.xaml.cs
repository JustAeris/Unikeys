using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using SharpVectors.Converters;

namespace Unikeys.Gui;

public partial class MessageBox
{
    private readonly Exception? _exception;
    private MessageBoxResult Result { get; set; } = MessageBoxResult.None;

    private MessageBox(string title, string message, MessageBoxIcons icon, MessageBoxButtons messageBoxButtons = MessageBoxButtons.Ok, Exception? exception = null)
    {
        InitializeComponent();
        Title = title;
        MessageTextBlock.Text = message;
        switch (icon)
        {
            case MessageBoxIcons.None:
                break;
            case MessageBoxIcons.Error:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "AlertOctagonIcon");
                break;
            case MessageBoxIcons.Info:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "InfoIcon");
                break;
            case MessageBoxIcons.Question:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "HelpIcon");
                break;
            case MessageBoxIcons.Warning:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "AlertTriangleIcon");
                break;
            case MessageBoxIcons.Success:
                IconSvgViewbox.SetResourceReference(SvgViewbox.SourceProperty, "CircleCheckIcon");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
        }

        switch (messageBoxButtons)
        {
            case MessageBoxButtons.Ok:
                OkButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButtons.OkCancel:
                OkButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButtons.YesNo:
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButtons.YesNoCancel:
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(messageBoxButtons), messageBoxButtons, null);
        }

        if (exception == null) return;
        ReportButton.Visibility = Visibility.Visible;
        _exception = exception;
    }

    public enum MessageBoxIcons
    {
        None,
        Error,
        Info,
        Question,
        Warning,
        Success
    }

    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    public enum MessageBoxResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Ok;
        Close();
    }

    private void NoButton_OnClick(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.No;
        Close();
    }

    private void YesButton_OnClick(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Yes;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        Close();
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

    public static MessageBoxResult Show(string title, string message, MessageBoxIcons icon = MessageBoxIcons.None, MessageBoxButtons buttons = MessageBoxButtons.Ok, Exception? exception = null)
    {
        var messageBox = new MessageBox(title, message, icon, buttons, exception);
        messageBox.ShowDialog();
        return messageBox.Result;
    }
}
