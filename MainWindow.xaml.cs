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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EncryptionTool
{
    /// <summary>
    /// Class that represents an AES key item with its name and file paths
    /// </summary>
    public class AesKeyItem
    {
        public string Name { get; set; }
        public string KeyPath { get; set; }
        public string IvPath { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }
    
    /// <summary>
    /// Main window class that handles the encryption and decryption operations
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _defaultKeyFolderPath;
        private string _ciphertextFolderPath;
        private string _plaintextFolderPath;
        private ObservableCollection<AesKeyItem> _aesKeys = new ObservableCollection<AesKeyItem>();
        private const string ImageFileFilter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
        private const string CiphertextFileFilter = "Encrypted files (*.enc)|*.enc";
        private const string DefaultImageExtension = ".png";
private static readonly string[] AllowedImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };


        /// <summary>
        /// Constructor for the MainWindow class
        /// Initializes UI components, sets up event handlers, and configures default folders
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Register event handler for extension ComboBox changes
            if (DecryptedExtensionComboBox != null)
                DecryptedExtensionComboBox.SelectionChanged += DecryptedExtensionComboBox_SelectionChanged;

            _defaultKeyFolderPath = Properties.Settings.Default.DefaultKeyFolder;
            if (string.IsNullOrEmpty(_defaultKeyFolderPath))
            {
                _defaultKeyFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Keys");
                Properties.Settings.Default.DefaultKeyFolder = _defaultKeyFolderPath;
                Properties.Settings.Default.Save();
            }
            
            _ciphertextFolderPath = Properties.Settings.Default.CiphertextFolder;
            if (string.IsNullOrEmpty(_ciphertextFolderPath))
            {
                _ciphertextFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ciphertext");
                Properties.Settings.Default.CiphertextFolder = _ciphertextFolderPath;
                Properties.Settings.Default.Save();
            }
            
            _plaintextFolderPath = Properties.Settings.Default.PlaintextFolder;
            if (string.IsNullOrEmpty(_plaintextFolderPath))
            {
                _plaintextFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plaintext");
                Properties.Settings.Default.PlaintextFolder = _plaintextFolderPath;
                Properties.Settings.Default.Save();
            }
            
            Directory.CreateDirectory(_defaultKeyFolderPath);
            Directory.CreateDirectory(_ciphertextFolderPath);
            Directory.CreateDirectory(_plaintextFolderPath);
            
            CiphertextFolderTextBox.Text = _ciphertextFolderPath;
            PlaintextFolderTextBox.Text = _plaintextFolderPath;
            
            AesKeyComboBox.ItemsSource = _aesKeys;
            LoadAesKeys();
        }

        /// <summary>
        /// Event handler for the Generate AES Button
        /// Creates a new AES key and IV, saves them to files, and updates the UI
        /// </summary>
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

        /// <summary>
        /// Event handler for the Generate RSA Button
        /// Creates a new RSA key pair (public and private), saves them to files, and updates the UI
        /// </summary>
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


        /// <summary>
        /// Event handler for the Set Default Key Folder menu item
        /// Opens a folder browser dialog and sets the selected folder as the default key storage location
        /// </summary>
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
        /// <summary>
        /// Loads all available AES keys from the keys folder
        /// Populates the AesKeyComboBox with the found keys
        /// </summary>
        private void LoadAesKeys()
        {
            _aesKeys.Clear();
            string aesKeysFolder = Path.Combine(_defaultKeyFolderPath, "AESKeys");
            
            if (!Directory.Exists(aesKeysFolder))
            {
                Directory.CreateDirectory(aesKeysFolder);
                return;
            }
            
            var keyFiles = Directory.GetFiles(aesKeysFolder, "*.key");
            foreach (var keyFile in keyFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(keyFile);
                string ivFile = Path.Combine(aesKeysFolder, $"{fileName}.iv");
                
                if (File.Exists(ivFile))
                {
                    _aesKeys.Add(new AesKeyItem
                    {
                        Name = fileName,
                        KeyPath = keyFile,
                        IvPath = ivFile
                    });
                }
            }
            
            if (_aesKeys.Count > 0)
            {
                AesKeyComboBox.SelectedIndex = 0;
            }
        }
        
        /// <summary>
        /// Event handler for the Refresh Keys button
        /// Reloads all AES keys from the keys folder
        /// </summary>
        private void RefreshKeysButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAesKeys();
        }
        
        /// <summary>
        /// Event handler for the Set Ciphertext Folder button
        /// Opens a folder browser dialog and sets the selected folder as the ciphertext storage location
        /// </summary>
        private void SetCiphertextFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Folder for Ciphertext Files",
                ShowNewFolderButton = true
            };
            
            if (Directory.Exists(_ciphertextFolderPath))
            {
                dialog.SelectedPath = _ciphertextFolderPath;
            }
            
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                _ciphertextFolderPath = dialog.SelectedPath;
                Properties.Settings.Default.CiphertextFolder = _ciphertextFolderPath;
                Properties.Settings.Default.Save();
                
                CiphertextFolderTextBox.Text = _ciphertextFolderPath;
                Directory.CreateDirectory(_ciphertextFolderPath);
                
                StatusTextBox.AppendText($"Ciphertext folder set to: {_ciphertextFolderPath}{Environment.NewLine}");
            }
        }
        
        /// <summary>
        /// Event handler for the Set Plaintext Folder button
        /// Opens a folder browser dialog and sets the selected folder as the plaintext storage location
        /// </summary>
        private void SetPlaintextFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Folder for Plaintext Files",
                ShowNewFolderButton = true
            };
            
            if (Directory.Exists(_plaintextFolderPath))
            {
                dialog.SelectedPath = _plaintextFolderPath;
            }
            
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                _plaintextFolderPath = dialog.SelectedPath;
                Properties.Settings.Default.PlaintextFolder = _plaintextFolderPath;
                Properties.Settings.Default.Save();
                
                PlaintextFolderTextBox.Text = _plaintextFolderPath;
                Directory.CreateDirectory(_plaintextFolderPath);
                
                StatusTextBox.AppendText($"Plaintext folder set to: {_plaintextFolderPath}{Environment.NewLine}");
            }
        }
        
        /// <summary>
        /// Event handler for the Select Ciphertext button
        /// Opens a file dialog to select an encrypted file for decryption
        /// Updates the UI with the selected file and suggested output filename
        /// </summary>
        private void SelectCiphertextButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = CiphertextFileFilter,
                Title = "Select a Ciphertext to Decrypt",
                InitialDirectory = _ciphertextFolderPath
            };
            
            if (dialog.ShowDialog() == true)
            {
                SelectedCiphertextPathTextBox.Text = dialog.FileName;
                string selectedExtension = GetSelectedDecryptedExtension();
                DecryptedFileNameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName) + selectedExtension;
            }
        }

        /// <summary>
        /// Gets the currently selected file extension from the DecryptedExtensionComboBox
        /// </summary>
        /// <returns>The selected extension string (e.g., ".png") or the default extension if none selected</returns>
        private string GetSelectedDecryptedExtension()
        {
            if (DecryptedExtensionComboBox != null && DecryptedExtensionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }
            return DefaultImageExtension;
        }

        /// <summary>
        /// Event handler for when the user changes the output file extension in the ComboBox
        /// Updates the decrypted file name with the new extension
        /// </summary>
        private void DecryptedExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(SelectedCiphertextPathTextBox.Text))
            {
                string baseName = Path.GetFileNameWithoutExtension(SelectedCiphertextPathTextBox.Text);
                string selectedExtension = GetSelectedDecryptedExtension();
                DecryptedFileNameTextBox.Text = baseName + selectedExtension;
            }
        }

        /// <summary>
        /// Event handler for the Select Image to Encrypt button
        /// Opens a file dialog to select an image file for encryption
        /// </summary>
        private void SelectImageToEncryptButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = ImageFileFilter,
                Title = "Select an Image to Encrypt"
            };
            
            if (dialog.ShowDialog() == true)
            {
                SelectedImagePathTextBox.Text = dialog.FileName;
            }
        }
        
        /// <summary>
        /// Encrypts data using AES algorithm with the provided key and initialization vector
        /// </summary>
        /// <param name="data">The raw data to encrypt</param>
        /// <param name="key">The AES key</param>
        /// <param name="iv">The initialization vector</param>
        /// <returns>Encrypted byte array</returns>
        private byte[] EncryptData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }
        
        /// <summary>
        /// Decrypts data using AES algorithm with the provided key and initialization vector
        /// </summary>
        /// <param name="data">The encrypted data</param>
        /// <param name="key">The AES key</param>
        /// <param name="iv">The initialization vector</param>
        /// <returns>Decrypted byte array</returns>
        private byte[] DecryptData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return ms.ToArray();
                    }
                }
            }
        }
        /// <summary>
        /// Event handler for the Encrypt button
        /// Performs validation, reads the selected image file, encrypts it using the selected AES key,
        /// and saves the encrypted data to the ciphertext folder
        /// </summary>
        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AesKeyItem selectedKey = AesKeyComboBox.SelectedItem as AesKeyItem;
                if (selectedKey == null)
                {
                    MessageBox.Show("Please select an AES key.", "Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(SelectedImagePathTextBox.Text) || !File.Exists(SelectedImagePathTextBox.Text))
                {
                    MessageBox.Show("Please select a valid image to encrypt.", "Image Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(EncryptedFileNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a name for the encrypted file.", "File Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                byte[] key = Convert.FromBase64String(File.ReadAllText(selectedKey.KeyPath));
                byte[] iv = Convert.FromBase64String(File.ReadAllText(selectedKey.IvPath));

                // Read the raw image data as bytes
                byte[] imageData = File.ReadAllBytes(SelectedImagePathTextBox.Text);

                // Perform AES encryption on the image data
                byte[] encryptedData = EncryptData(imageData, key, iv);

                // Create the output file path and save the encrypted data
                string outputPath = Path.Combine(_ciphertextFolderPath, EncryptedFileNameTextBox.Text + ".enc");
                File.WriteAllBytes(outputPath, encryptedData);
                StatusTextBox.AppendText($"Encrypted file saved to: {outputPath}{Environment.NewLine}");
                MessageBox.Show("Encryption successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during encryption: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBox.AppendText($"Encryption error: {ex.Message}{Environment.NewLine}");
            }
        }

        /// <summary>
        /// Event handler for the Decrypt button
        /// Performs validation, reads the selected encrypted file, decrypts it using the selected AES key,
        /// and saves the decrypted data to the plaintext folder with the specified extension
        /// </summary>
        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AesKeyItem selectedKey = AesKeyComboBox.SelectedItem as AesKeyItem;
                if (selectedKey == null)
                {
                    MessageBox.Show("Please select an AES key.", "Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(SelectedCiphertextPathTextBox.Text) || !File.Exists(SelectedCiphertextPathTextBox.Text))
                {
                    MessageBox.Show("Please select a valid ciphertext file to decrypt.", "Ciphertext Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(DecryptedFileNameTextBox.Text))
                {
                    MessageBox.Show("Please enter a name for the decrypted file.", "File Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                byte[] key = Convert.FromBase64String(File.ReadAllText(selectedKey.KeyPath));
                byte[] iv = Convert.FromBase64String(File.ReadAllText(selectedKey.IvPath));

                // Read the encrypted data from the selected file
                byte[] encryptedData = File.ReadAllBytes(SelectedCiphertextPathTextBox.Text);

                // Perform AES decryption on the encrypted data
                byte[] decryptedData = DecryptData(encryptedData, key, iv);

                // Get the selected output extension and create the output file path
                string extension = GetSelectedDecryptedExtension();
                string outputPath = Path.Combine(_plaintextFolderPath, DecryptedFileNameTextBox.Text);
                // Make sure the filename has the correct extension
                if (!outputPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    outputPath += extension;
                // Save the decrypted data as an image file
                File.WriteAllBytes(outputPath, decryptedData);
                StatusTextBox.AppendText($"Decrypted file saved to: {outputPath}{Environment.NewLine}");
                MessageBox.Show("Decryption successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during decryption: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBox.AppendText($"Decryption error: {ex.Message}{Environment.NewLine}");
            }
        }
    }
}
