using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Unikeys.Core.FileEncryption;

namespace Unikeys.Gui.Tabs;

public partial class EncryptTab
{
    private string _filePath;

    public EncryptTab()
    {
        InitializeComponent();
        _filePath = "";
    }

    /// <summary>
    /// Allows the user to select a file to encrypt
    /// </summary>
    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose file to encrypt",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

         (_filePath, FilePathTextBox.Text) = (dialog.FileName, dialog.FileName);
    }

    /// <summary>
    /// Shows some help about the unique key feature
    /// </summary>
    private void HelpButton_OnClick(object sender, RoutedEventArgs e) =>
        MessageBox.Show("Quick tip!",
            "If you don't specify a password, the program will generate for you a strong decryption key, but you will be able to see it only once, so make sur to save it in time!",
            MessageBox.MessageBoxIcons.Info);


    /// <summary>
    /// Encrypts the file with the specified password
    /// </summary>
    private async void EncryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file has been chosen
        if (_filePath == "")
        {
            MessageBox.Show("Oops...", "You must choose a file to encrypt!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Check if the encrypted file already exists
        if (File.Exists(_filePath + ".unikeys"))
        {
            MessageBox.Show("Oops...", "The file you want to encrypt already exists!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Ask the user where to save the file
        var dialog = new SaveFileDialog
        {
            Title = "Choose where to save the encrypted file",
            CheckPathExists = true,
            OverwritePrompt = true,
            AddExtension = true,
            FileName = _filePath + ".unikeys",
            Filter = "Unikeys file (*.unikeys)|*.unikeys"
        };

        dialog.ShowDialog();

        // Abort if the user didn't choose a file and show a warning message
        if (dialog.FileName == "")
        {
            MessageBox.Show("Oops...", "You must choose a file to save the encrypted file!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Encrypt the file
        var key = PasswordInputBox.Password;
        LockEncryptionGui(true);
        try
        {
            await Task.Run(() => key = EncryptionDecryption.EncryptFile(_filePath, dialog.FileName, PasswordInputBox.Password));
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                "Oops...", "Something went wrong during encryption", MessageBox.MessageBoxIcons.Error,
                exception: exception);
            return;
        }
        finally
        {
            LockEncryptionGui(false);
        }

        // Display a message box to confirm the encryption
        if (PasswordInputBox.Password != key)
            new UniqueKeyDisplayWindow(key).Show();
        else
            MessageBox.Show("Success!", "The file has been encrypted successfully!",
                MessageBox.MessageBoxIcons.Success);

        // Clear the text boxes
        FilePathTextBox.Text = "";
        PasswordInputBox.Password = "";

        // Clear the file path
        _filePath = "";
    }

    /// <summary>
    /// Lock the GUI during the encryption process
    /// </summary>
    /// <param name="locked">Whether to lock or not</param>
    private void LockEncryptionGui(bool locked)
    {
        PasswordInputBox.IsEnabled = !locked;
        ChooseFileButton.IsEnabled = !locked;
        EncryptButton.IsEnabled = !locked;

        // Show a loading animation while the encryption is in progress
        EncryptButtonContent.Visibility = locked ? Visibility.Collapsed : Visibility.Visible;
        ProgressIndicator.Visibility = locked ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Copies the current password to the clipboard
    /// </summary>
    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Clipboard.GetText() == PasswordInputBox.Password || PasswordInputBox.Password == "") return;

        Clipboard.SetText(PasswordInputBox.Password);
        _ = Task.Run(() => Dispatcher.Invoke(async () => await ShowToolTip((Button)sender, "Copied to clipboard!")));
    }

    /// <summary>
    /// Generates a strong password
    /// </summary>
    private void GeneratePasswordButton_OnClick(object sender, RoutedEventArgs e)
    {
        PasswordInputBox.Password = PasswordGenerator.GetNewPassword();
        _ = Task.Run(() => Dispatcher.Invoke(async () => await ShowToolTip((Button)sender, "A new password has been generated!")));
    }

    /// <summary>
    /// Show a tooltip to act a popup.
    /// </summary>
    /// <param name="sender">Control where the tooltip is</param>
    /// <param name="text">Text to display inside. Previous content will be restored on tooltip's closing</param>
    private static async Task ShowToolTip(Control sender, string text)
    {
        var toolTip = new ToolTip();

        if (sender.ToolTip != null)
        {
            var previousText = sender.ToolTip;

            toolTip.Content = text;
            toolTip.StaysOpen = true;
            toolTip.IsOpen = true;

            await Task.Delay(1500);

            toolTip.StaysOpen = false;
            toolTip.IsOpen = false;
            await Task.Delay(100);
            toolTip.Content = previousText;
        }
    }
}
