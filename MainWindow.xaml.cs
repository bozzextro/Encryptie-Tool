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

namespace EncryptionTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateAesButton_Click(object sender, RoutedEventArgs e)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();

                string base64Key = Convert.ToBase64String(aes.Key);
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AESKeys");
                Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, $"{KeyNameTextBox.Text}.txt");

                File.WriteAllText(filePath, base64Key);

                MessageBox.Show($"AES key succesfully generated and saved to: {filePath}");
            }
        }

        private void GenerateRsaButton_Click(object sender, RoutedEventArgs e)
        {
            string keyName = KeyNameTextBox.Text;

            using (RSA rsa = RSA.Create(2048))
            {
                string privateKey = rsa.ToXmlString(true);
                string publicKey = rsa.ToXmlString(false);

                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RSAKeys");
                Directory.CreateDirectory(folderPath);

                string privateKeyPath = Path.Combine(folderPath, $"{keyName}_private.xml");
                string publicKeyPath = Path.Combine(folderPath, $"{keyName}_public.xml");

                File.WriteAllText(privateKeyPath, privateKey);
                File.WriteAllText(publicKeyPath, publicKey);

                MessageBox.Show($"RSA keys succesfully generated and saved to: {publicKeyPath}");
            }
        }
    }
}