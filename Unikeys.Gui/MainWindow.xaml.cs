using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Unikeys.Core;
using ModernWpf;

namespace Unikeys.Gui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public partial class MainWindow
{
    /// <summary>
    /// Window constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        // Dark theme will come soon
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

        VersionLabel.Content += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }

    /// <summary>
    /// Allows the user to select a file to encrypt
    /// </summary>
    private void ChooseFileEncryptionButton_OnClick(object sender, RoutedEventArgs e)
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

        FilePathEncryptionTextBox.Text = dialog.FileName;
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
    private void EncryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file has been chosen
        if (FilePathEncryptionTextBox.Text == "")
        {
            new CustomMessageBox("Oops...", "You must choose a file to encrypt!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if the encrypted file already exists
        if (File.Exists(FilePathEncryptionTextBox.Text + ".unikeys"))
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
            FileName = FilePathEncryptionTextBox.Text + ".unikeys",
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

        var key = PasswordBox.Password;
        LockEncryptionGui(true);
        try
        {
            key = EncryptDecrypt.EncryptFile(FilePathEncryptionTextBox.Text, dialog.FileName, PasswordBox.Password);
        }
        catch (Exception exception)
        {
            new CustomMessageBox(
                "Oops...", "Something went wrong during encryption", CustomMessageBox.CustomMessageBoxIcons.Error,
                exception).Show();
        }
        finally
        {
            LockEncryptionGui(false);
        }

        // Display a message box to confirm the encryption
        if (PasswordBox.Password != key)
            new UniqueKeyDisplayWindow(key).Show();
        else
            new CustomMessageBox("Success!", "The file has been encrypted successfully!",
                CustomMessageBox.CustomMessageBoxIcons.Success).Show();

        // Clear the text boxes
        FilePathEncryptionTextBox.Text = "";
        PasswordBox.Password = "";
    }

    /// <summary>
    /// Lock the GUI during the encryption process
    /// </summary>
    /// <param name="locked">Whether to lock or not</param>
    private void LockEncryptionGui(bool locked)
    {
        PasswordBox.IsEnabled = !locked;
        ChooseFileEncryptionButton.IsEnabled = !locked;
        EncryptButton.IsEnabled = !locked;
    }

    /// <summary>
    /// Allows the user to choose a unikeys file to decrypt
    /// </summary>
    private void ChooseFileDecryptionButton_OnClick(object sender, RoutedEventArgs e)
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

        FilePathDecryptionTextBox.Text = dialog.FileName;
    }

    /// <summary>
    /// Decrypts the file with the specified password
    /// </summary>
    private void DecryptButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file is selected
        if (FilePathDecryptionTextBox.Text == "")
        {
            new CustomMessageBox("Oops...", "You must choose a file to decrypt!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if the file is a unikeys file
        if (!FilePathDecryptionTextBox.Text.EndsWith(".unikeys"))
        {
            new CustomMessageBox("Oops...", "The file you want to decrypt is not a unikeys file!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if a password is specified
        if (KeyPasswordBox.Password == "")
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
            FileName = FilePathDecryptionTextBox.Text.Replace(".unikeys", ""),
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

        if (dialog.FileName == "")
        {
            // Abort the decryption if the user didn't choose a file and show a message
            new CustomMessageBox("Oops...", "You must specify where to save the file!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        LockDecryptionGui(true);
        try
        {
            EncryptDecrypt.DecryptFile(FilePathDecryptionTextBox.Text, dialog.FileName, KeyPasswordBox.Password);
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
        FilePathDecryptionTextBox.Text = "";
        KeyPasswordBox.Password = "";
    }

    /// <summary>
    /// Lock the GUI during the decryption process
    /// </summary>
    /// <param name="locked">Whether to lock or not</param>
    private void LockDecryptionGui(bool locked)
    {
        KeyPasswordBox.IsEnabled = !locked;
        ChooseFileDecryptionButton.IsEnabled = !locked;
        DecryptButton.IsEnabled = !locked;
    }


    private void ChooseFileShredButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose files to shred",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = true,
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

        FileListView.ItemsSource = dialog.FileNames;
    }

    private async void ShredButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (FileListView.Items.Count == 0)
        {
            new CustomMessageBox("Oops...", "You must choose at least one file to shred!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        Tabs.IsEnabled = false;
        var dialog = new ConfirmShredWindow();
        dialog.ShowDialog();
        Tabs.IsEnabled = true;

        if (dialog.Confirmed != true) return;

        if (FileListView.ItemsSource is not string[] files)
            return;

        LockShredGui(true);
        try
        {
            var filesToShred = files.Select(f => new FileInfo(f));
            await SDelete.DeleteFiles(filesToShred);
        }
        catch (Exception exception)
        {
            new CustomMessageBox("Oops...", "Something went wrong while shredding the files!",
                CustomMessageBox.CustomMessageBoxIcons.Error, exception).Show();
            return;
        }
        finally
        {
            LockShredGui(false);
        }

        new CustomMessageBox("Success!", "Files shredded successfully!",
            CustomMessageBox.CustomMessageBoxIcons.Success).Show();

        FileListView.ItemsSource = null;
    }

    private void LockShredGui(bool locked)
    {
        ChooseFileShredButton.IsEnabled = !locked;
        ShredButton.IsEnabled = !locked;
    }
}
