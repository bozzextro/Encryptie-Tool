using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Configuration;

namespace EncryptionTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _defaultKeyFolderPath;

        public MainWindow()
        {
            InitializeComponent();

            _defaultKeyFolderPath = Properties.Settings.Default.DefaultKeyFolder;
            if (string.IsNullOrEmpty(_defaultKeyFolderPath))
            {
                _defaultKeyFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Keys");
                Properties.Settings.Default.DefaultKeyFolder = _defaultKeyFolderPath;
                Properties.Settings.Default.Save();
            }
            
            Directory.CreateDirectory(_defaultKeyFolderPath);
        }

        private void GenerateAesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeyNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the key", "Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();

                string base64Key = Convert.ToBase64String(aes.Key);
                string base64IV = Convert.ToBase64String(aes.IV);

                string aesKeysFolderPath = Path.Combine(_defaultKeyFolderPath, "AESKeys");
                Directory.CreateDirectory(aesKeysFolderPath);
                string keyFileName = $"{KeyNameTextBox.Text}.key";
                string ivFileName = $"{KeyNameTextBox.Text}.iv";
                string keyFilePath = Path.Combine(aesKeysFolderPath, keyFileName);
                string ivFilePath = Path.Combine(aesKeysFolderPath, ivFileName);

                File.WriteAllText(keyFilePath, base64Key);
                File.WriteAllText(ivFilePath, base64IV);

                MessageBox.Show($"AES key and IV successfully generated and saved to:{Environment.NewLine}{keyFilePath}{Environment.NewLine}{ivFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateRsaButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(KeyNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the key", "Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string keyName = KeyNameTextBox.Text;

            using (RSA rsa = RSA.Create(2048))
            {
                string privateKey = rsa.ToXmlString(true);
                string publicKey = rsa.ToXmlString(false);

                string rsaKeysFolderPath = Path.Combine(_defaultKeyFolderPath, "RSAKeys");
                Directory.CreateDirectory(rsaKeysFolderPath);

                string privateKeyPath = Path.Combine(rsaKeysFolderPath, $"{keyName}_private.xml");
                string publicKeyPath = Path.Combine(rsaKeysFolderPath, $"{keyName}_public.xml");

                File.WriteAllText(privateKeyPath, privateKey);
                File.WriteAllText(publicKeyPath, publicKey);

                MessageBox.Show($"RSA keys successfully generated and saved to:{Environment.NewLine}{publicKeyPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void SetDefaultKeyFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Default Folder for Key Storage",
                ShowNewFolderButton = true
            };

            if (Directory.Exists(_defaultKeyFolderPath))
            {
                dialog.SelectedPath = _defaultKeyFolderPath;
            }

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                _defaultKeyFolderPath = dialog.SelectedPath;

                Properties.Settings.Default.DefaultKeyFolder = _defaultKeyFolderPath;
                Properties.Settings.Default.Save();
                
                Directory.CreateDirectory(Path.Combine(_defaultKeyFolderPath, "AESKeys"));
                Directory.CreateDirectory(Path.Combine(_defaultKeyFolderPath, "RSAKeys"));

                MessageBox.Show($"Default key folder set to:{Environment.NewLine}{_defaultKeyFolderPath}", "Folder Set", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}