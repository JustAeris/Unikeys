using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Unikeys.Core.FileShredding;

namespace Unikeys.Gui.Tabs;

public partial class ShredTab
{
    public ShredTab() => InitializeComponent();

    /// <summary>
    /// Allows the user to select files to shred.
    /// </summary>
    private void ChooseFilesButton_OnClick(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// Proceeds with the shredding process.
    /// </summary>
    private async void ShredButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (FileListView.Items.Count == 0)
        {
            MessageBox.Show("Oops...", "You must choose at least one file to shred!",
                MessageBox.MessageBoxIcons.Warning);
            return;
        }

        LockShredGui(true);
        var dialog = new ConfirmShredWindow();
        dialog.ShowDialog();
        LockShredGui(false);

        if (dialog.Confirmed != true) return;

        if (FileListView.ItemsSource is not string[] files)
            return;

        LockShredGui(true);
        try
        {
            var filesToShred = files.Select(f => new FileInfo(f));
            await Task.Run(() => SDelete.DeleteFiles(filesToShred));
        }
        catch (Exception exception)
        {
            MessageBox.Show("Oops...", "Something went wrong while shredding the files!",
                MessageBox.MessageBoxIcons.Error, exception: exception);
            return;
        }
        finally
        {
            LockShredGui(false);
        }

        MessageBox.Show("Success!", "Files shredded successfully!",
            MessageBox.MessageBoxIcons.Success);

        FileListView.ItemsSource = null;
    }

    /// <summary>
    /// Locks the GUI while shredding.
    /// </summary>
    /// <param name="locked"></param>
    private void LockShredGui(bool locked)
    {
        ChooseFilesButton.IsEnabled = !locked;
        ShredButton.IsEnabled = !locked;
        FileListView.IsEnabled = !locked;

        // Show a loading animation while shredding
        ShredButtonContent.Visibility = locked ? Visibility.Collapsed : Visibility.Visible;
        ProgressIndicator.Visibility = locked ? Visibility.Visible : Visibility.Collapsed;
    }
}
