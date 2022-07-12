using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using Microsoft.Win32;
using ModernWpf;

namespace Unikeys.Gui
{
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

            var aes = Aes.Create();

            // Derive a key from the password and a salt
            var key = PasswordBox.Password == "" ?
                Rfc2898DeriveBytes.Pbkdf2(aes.Key, aes.IV, 1069, HashAlgorithmName.SHA512, aes.KeySize / 8) :
                Rfc2898DeriveBytes.Pbkdf2(PasswordBox.Password, aes.IV, 1069, HashAlgorithmName.SHA512, aes.KeySize / 8);
            aes.Key = key;

            byte[] result;

            // Encrypt the file
            LockEncryptionGui(true);
            try
            {
                result = aes.EncryptCbc(File.ReadAllBytes(FilePathEncryptionTextBox.Text), aes.IV);
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Oops...", "Something went wrong while encrypting the file!",
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                new CustomMessageBox("Error details:", ex.Message,
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                return;
            }
            finally
            {
                LockEncryptionGui(false);
            }

            // Add the IV to the result
            Array.Resize(ref result, result.Length + aes.IV.Length);
            Array.Copy(aes.IV, 0, result, result.Length - aes.IV.Length, aes.IV.Length);

            // Ask the user where to save the file
            var dialog = new SaveFileDialog
            {
                Title = "Choose where to save the encrypted file",
                CheckPathExists = true,
                OverwritePrompt = true,
                AddExtension = true,
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

            // Save the encrypted file
            LockEncryptionGui(true);
            try
            {
                File.WriteAllBytes(FilePathEncryptionTextBox.Text + ".unikeys", result);
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Oops...", "Something went wrong while saving the file!",
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                new CustomMessageBox("Error details:", ex.Message,
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                return;
            }
            finally
            {
                LockEncryptionGui(false);
                Array.Clear(result);
            }

            // Display a message box to confirm the encryption
            if (PasswordBox.Password == "")
                new UniqueKeyDisplayWindow(Convert.ToBase64String(key)).Show();
            else
                new CustomMessageBox("Success!", "The file has been encrypted successfully!",
                    CustomMessageBox.CustomMessageBoxIcons.Success).Show();
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
        /// Allows the user to choose a file to decrypt
        /// </summary>
        private void ChooseFileDecryptionButton_OnClick(object sender, RoutedEventArgs e)
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

            // Check if the file already exists
            if (File.Exists(FilePathDecryptionTextBox.Text.Replace(".unikeys", "") ))
            {
                new CustomMessageBox("Oops...", "The file already exists!",
                    CustomMessageBox.CustomMessageBoxIcons.Warning).Show();
                return;
            }

            // Get the last 16 bytes of fileBytes to get the IV
            var fileBytes = File.ReadAllBytes(FilePathDecryptionTextBox.Text);
            var iv = new byte[16];
            Array.Copy(fileBytes, fileBytes.Length - 16, iv, 0, 16);

            var aes = Aes.Create();

            var tryBase64 = Array.Empty<byte>();
            try
            {
                tryBase64 = Convert.FromBase64String(KeyPasswordBox.Password);
            }
            catch
            {
                // Ignored ; I use a try/catch here because TryFromBase64String is janky and doesn't work
            }

            var key = tryBase64.Length == 32 ? tryBase64.ToArray() : Rfc2898DeriveBytes.Pbkdf2(KeyPasswordBox.Password, iv, 1069, HashAlgorithmName.SHA512, aes.KeySize / 8);

            aes.Key = key;
            aes.IV = iv;

            // Decrypt the fileBytes except the last 16 bytes (IV)
            byte[] result;
            LockDecryptionGui(true);
            try
            {
                result = aes.DecryptCbc(fileBytes.Take(fileBytes.Length - 16).ToArray(), iv);
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Oops...", "Something went wrong while decrypting the file! Maybe a wrong password?",
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                new CustomMessageBox("Error details:", ex.Message,
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                return;
            }
            finally
            {
                LockDecryptionGui(false);
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

            // Write the result to a file
            try
            {
                File.WriteAllBytes(dialog.FileName.Replace(".unikeys", ""), result);
            }
            catch (Exception ex)
            {
                new CustomMessageBox("Oops...", "Something went wrong while saving the file!",
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                new CustomMessageBox("Error details:", ex.Message,
                    CustomMessageBox.CustomMessageBoxIcons.Error).Show();
                return;
            }
            finally
            {
                Array.Clear(result);
            }

            new CustomMessageBox("Success!", "File decrypted successfully!",
                CustomMessageBox.CustomMessageBoxIcons.Success).Show();
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
    }
}
