using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Unikeys.Core.FileEncryption;

namespace Unikeys.Gui.Tabs;

public partial class DecryptTab
{
    private string _filePath;

    public DecryptTab()
    {
        InitializeComponent();
        _filePath = "";
    }

    /// <summary>
    /// Allows the user to choose a unikeys file to decrypt
    /// </summary>
    private void ChooseFileButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose file to encrypt",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "Unikeys files (*.unikeys)|*.unikeys"
        };

        dialog.ShowDialog();

        (FilePathTextBox.Text, _filePath) = (dialog.FileName, dialog.FileName);
    }

    /// <summary>
    /// Decrypts the file with the specified password
    /// </summary>
    private async void DecryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file is selected
        if (_filePath == "")
        {
            new CustomMessageBox("Oops...", "You must choose a file to decrypt!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if the file is a unikeys file
        if (!_filePath.EndsWith(".unikeys"))
        {
            new CustomMessageBox("Oops...", "The file you want to decrypt is not a unikeys file!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if a password is specified
        if (PasswordInputBox.Password == "")
        {
            new CustomMessageBox("Oops...", "You must specify a key password!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Ask the user where to save the file
        var dialog = new SaveFileDialog
        {
            Title = "Choose file to decrypt",
            CheckPathExists = true,
            CheckFileExists = false,
            OverwritePrompt = true,
            FileName = _filePath.Replace(".unikeys", ""),
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

        // Abort the decryption if the user didn't choose a file and show a message
        if (dialog.FileName == "")
        {
            new CustomMessageBox("Oops...", "You must specify where to save the file!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Try to decrypt the file
        LockDecryptionGui(true);
        try
        {
            await Task.Run(() => Decryption.DecryptFile(_filePath, dialog.FileName, PasswordInputBox.Password));
        }
        catch (Exception ex)
        {
            new CustomMessageBox("Oops...", "Something went wrong while decrypting the file! Maybe a wrong password?",
                CustomMessageBox.CustomMessageBoxIcons.Error, ex).Show();
            return;
        }
        finally
        {
            LockDecryptionGui(false);
        }

        new CustomMessageBox("Success!", "File decrypted successfully!",
            CustomMessageBox.CustomMessageBoxIcons.Success).Show();

        // Clear the text boxes
        FilePathTextBox.Text = "";
        PasswordInputBox.Password = "";

        // Clear the file path
        _filePath = "";
    }

    /// <summary>
    /// Lock the GUI during the decryption process
    /// </summary>
    /// <param name="locked">Whether to lock or not</param>
    private void LockDecryptionGui(bool locked)
    {
        PasswordInputBox.IsEnabled = !locked;
        ChooseFileButton.IsEnabled = !locked;
        DecryptButton.IsEnabled = !locked;
    }
}
