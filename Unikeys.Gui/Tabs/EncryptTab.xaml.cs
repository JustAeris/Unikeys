using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
        new CustomMessageBox("Quick tip!",
            "If you don't specify a password, the program will generate for you a strong decryption key, but you will be able to see it only once, so make sur to save it in time!",
            CustomMessageBox.CustomMessageBoxIcons.Info).Show();


    /// <summary>
    /// Encrypts the file with the specified password
    /// </summary>
    private async void EncryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file has been chosen
        if (_filePath == "")
        {
            new CustomMessageBox("Oops...", "You must choose a file to encrypt!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if the encrypted file already exists
        if (File.Exists(_filePath + ".unikeys"))
        {
            new CustomMessageBox("Oops...", "The file you want to encrypt already exists!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
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
            new CustomMessageBox("Oops...", "You must choose a file to save the encrypted file!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Encrypt the file
        var key = PasswordInputBox.Password;
        LockEncryptionGui(true);
        try
        {
            await Task.Run(() => key = Encryption.EncryptFile(_filePath, dialog.FileName, PasswordInputBox.Password));
        }
        catch (Exception exception)
        {
            new CustomMessageBox(
                "Oops...", "Something went wrong during encryption", CustomMessageBox.CustomMessageBoxIcons.Error,
                exception).Show();
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
            new CustomMessageBox("Success!", "The file has been encrypted successfully!",
                CustomMessageBox.CustomMessageBoxIcons.Success).Show();

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
    }
}
