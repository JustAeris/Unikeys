using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Unikeys.Core.FileEncryption;

namespace Unikeys.Gui.Tabs;

public partial class SelfExeTab
{
    private string _filePath;

    public SelfExeTab()
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

    private async void MakeSFXButton_OnClick(object sender, RoutedEventArgs e)
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

        LockSFXGui(true);
        try
        {
            await Task.Run(() => SFX.MakeSFX(_filePath,
                Path.Combine(new FileInfo(_filePath).Directory?.FullName ?? string.Empty,
                    new FileInfo(_filePath).Name + ".exe")));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Oops...", "Something went wrong while decrypting the file! Maybe a wrong password?",
                MessageBox.MessageBoxIcons.Error, exception: ex);
            return;
        }
        finally
        {
            LockSFXGui(false);
        }

        MessageBox.Show("Success!", "The file was successfully made into a SFX!");
    }

    private void LockSFXGui(bool locked)
    {
        ChooseFileButton.IsEnabled = !locked;
        MakeSFXButton.IsEnabled = !locked;

        // Shows a loading animation while the decryption is in progress
        MakeSFXButtonContent.Visibility = locked ? Visibility.Collapsed : Visibility.Visible;
        ProgressIndicator.Visibility = locked ? Visibility.Visible : Visibility.Collapsed;
    }
}
