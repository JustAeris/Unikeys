using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Unikeys.Core.FileSigning;
using Unikeys.Core.FolderWatcher;

namespace Unikeys.Gui.Tabs;

public partial class SignVerifyTab
{
    private string _fileToSignPath = "";
    private string _fileToVerifyPath = "";
    private FileInfo? _signatureFile;

    public SignVerifyTab()
    {
        InitializeComponent();

        var watcher = new CertificatesFolderWatcher();
        _ = Task.Run(() => watcher.Start()) ;

        CertificateListBox.ItemsSource = watcher.CertificatesList;
        CertificateListComboBox.ItemsSource = watcher.CertificatesList;

        watcher.CertificatesListUpdated += (_, args) =>
        {
            Dispatcher.Invoke(() =>
            {
                CertificateListBox.ItemsSource = args.CertificatesList;
                CertificateListComboBox.ItemsSource = args.CertificatesList;
            });
        };
    }

    /// <summary>
    /// Allows the creation of a certificate containing a RSA key pair
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddCertificateButton_OnClick(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory("Certificates");

        // Show a save dialog to choose where to save the certificate
        var dialog = new SaveFileDialog
        {
            Title = "Choose file to save the certificate",
            CheckPathExists = true,
            CheckFileExists = false,
            OverwritePrompt = true,
            InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Certificates"),
            AddExtension = true,
            Filter = "PFX files (*.pfx)|*.pfx"
        };

        dialog.ShowDialog();

        if (dialog.FileName == "")
            return;

        // Save a new certificate using X509Helper
        var cert = X509Helper.GenerateX509Certificate("Unikeys");
        X509Helper.WriteX509Certificate(cert, dialog.FileName);

        new CustomMessageBox("Success!", "Certificate created successfully!",
            CustomMessageBox.CustomMessageBoxIcons.Success).Show();
    }

    /// <summary>
    /// Choose a file to sign
    /// </summary>
    private void ChooseFileSignButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose file to sign",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

        (FilePathSignTextBox.Text, _fileToSignPath) = (dialog.FileName, dialog.FileName);
    }

    /// <summary>
    /// Proceed to create a signature file
    /// </summary>
    private void SignButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file is specified
        if (_fileToSignPath == "")
        {
            new CustomMessageBox("Oops...", "You must specify a file to sign!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if a certificate is specified
        if (CertificateListComboBox.SelectedItem == null)
        {
            new CustomMessageBox("Oops...", "You must specify a certificate to sign with!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Sign the file
        try
        {
            var cert = X509Helper.GetCertificateFromPfx(((FileInfo)CertificateListComboBox.SelectedItem).FullName);
            var signature = RSASigning.SignData(File.ReadAllBytes(_fileToSignPath), cert);
            File.WriteAllText(_fileToSignPath + ".sig.xml", signature.ToXmlString());
        }
        catch (Exception exception)
        {
            new CustomMessageBox("Oops...", "Something went wrong while signing the file!",
                CustomMessageBox.CustomMessageBoxIcons.Error, exception).Show();
            return;
        }

        new CustomMessageBox("Success!", "Created a signature successfully!",
            CustomMessageBox.CustomMessageBoxIcons.Success).Show();

        // Clear the text boxes
        FilePathSignTextBox.Text = "";
        CertificateListComboBox.SelectedItem = null;
    }

    /// <summary>
    /// Choose a file to verify
    /// </summary>
    private void ChooseFileVerifyButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose file to verify",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "All files (*.*)|*.*"
        };

        dialog.ShowDialog();

        (FilePathVerifyTextBox.Text, _fileToVerifyPath) = (dialog.FileName, dialog.FileName);
    }

    /// <summary>
    /// Choose a signature file for comparison
    /// </summary>
    private void ChooseSignatureVerifyButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose signature to verify",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            Filter = "XML signature files (*.sig.xml)|*.sig.xml"
        };

        dialog.ShowDialog();

        _signatureFile = new FileInfo(dialog.FileName);
        SignaturePathVerifyTextBox.Text = _signatureFile.Name;
    }

    /// <summary>
    /// Compare the signature of the file with the file.
    /// </summary>
    private void VerifyButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Check if a file is specified
        if (_fileToVerifyPath == "")
        {
            new CustomMessageBox("Oops...", "You must specify a file to verify!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Check if a signature is specified
        if (_signatureFile == null)
        {
            new CustomMessageBox("Oops...", "You must specify a signature to verify!",
                CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
            return;
        }

        // Verify the file
        bool isValid;
        try
        {
            var signature = RSASignature.FromXmlString(File.ReadAllText(_signatureFile.FullName));
            isValid = RSASigning.VerifySignature(File.ReadAllBytes(_fileToVerifyPath), signature);
        }
        catch (Exception exception)
        {
            new CustomMessageBox("Oops...", "Something went wrong while verifying the file!",
                CustomMessageBox.CustomMessageBoxIcons.Error, exception).Show();
            return;
        }

        if (isValid)
        {
            new CustomMessageBox("Success!", "Signature is valid! The file has not been modified.",
                CustomMessageBox.CustomMessageBoxIcons.Success).Show();
            FilePathVerifyTextBox.Text = "";
            SignaturePathVerifyTextBox.Text = "";
        }
        else
            new CustomMessageBox("Oops...", "Signature is invalid! Either the file or the signature has been modified.",
                CustomMessageBox.CustomMessageBoxIcons.Error).Show();

        // Clear the text boxes
        FilePathVerifyTextBox.Text = "";
        SignaturePathVerifyTextBox.Text = "";

        _signatureFile = null;
        _fileToVerifyPath = "";
    }

    /// <summary>
    /// Prevents a item from being selected in the certificate list.
    /// </summary>
    private void CertificateListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => CertificateListBox.SelectedItem = null;
}
