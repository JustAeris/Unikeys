﻿using System;
using System.IO;
using System.Security.Cryptography;
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
            MessageBox.Show("Oops...", "You must choose a file to decrypt!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Check if the file is a unikeys file
        if (!_filePath.EndsWith(".unikeys"))
        {
            MessageBox.Show("Oops...", "The file you want to decrypt is not a unikeys file!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Check if a password is specified
        if (PasswordInputBox.Password == "")
        {
            MessageBox.Show("Oops...", "You must specify a key password!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        var info = new FileInfo(_filePath.Replace(".unikeys", ""));
        // Ask the user where to save the file
        var dialog = new SaveFileDialog
        {
            Title = "Choose file to decrypt",
            CheckPathExists = true,
            CheckFileExists = false,
            OverwritePrompt = true,
            InitialDirectory = info.Directory?.FullName,
            FileName = info.Name,
            Filter = $"{info.Extension.Replace(".", "").ToUpper()} files (*{info.Extension})|*{info.Extension}",
            AddExtension = true
        };

        dialog.ShowDialog();

        // Abort the decryption if the user didn't choose a file and show a message
        if (dialog.FileName == "")
        {
            MessageBox.Show("Oops...", "You must specify where to save the file!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        // Try to decrypt the file
        LockDecryptionGui(true);
        try
        {
            await Task.Run(() => EncryptionDecryption.DecryptFile(_filePath, dialog.FileName, PasswordInputBox.Password));
        }
        catch (CryptographicException ex)
        {
            if (ex.Message == "HMAC verification failed, file may have been tampered with")
                MessageBox.Show("Oops...", "Data integrity verification failed, file may have been tampered with!",
                    MessageBox.MessageBoxIcons.Warning);
            else
                MessageBox.Show("Oops...", "Something went wrong while decrypting the file! Maybe a wrong password?",
                    MessageBox.MessageBoxIcons.Error, exception: ex);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Oops...", "Something went wrong while decrypting the file! Maybe a wrong password?",
                MessageBox.MessageBoxIcons.Error, exception: ex);
            return;
        }
        finally
        {
            LockDecryptionGui(false);
        }

        MessageBox.Show("Success!", "File decrypted successfully!",
            MessageBox.MessageBoxIcons.Success);

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

        // Shows a loading animation while the decryption is in progress
        DecryptButtonContent.Visibility = locked ? Visibility.Collapsed : Visibility.Visible;
        ProgressIndicator.Visibility = locked ? Visibility.Visible : Visibility.Collapsed;
    }
}
