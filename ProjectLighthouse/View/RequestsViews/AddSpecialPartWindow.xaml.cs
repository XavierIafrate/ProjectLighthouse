using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public partial class AddSpecialPartWindow : Window
    {

        public string filename { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public bool submitted { get; set; }

        public AddSpecialPartWindow()
        {
            submitted = false;
            InitializeComponent();
            SetValues();
            fileWarning.Visibility = Visibility.Collapsed;
        }

        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            submitted = true;
            Close();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";

            string openDir = "H:\\Sales Office";

            if (!Directory.Exists(openDir))
                openDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                fileWarning.Visibility = filename[0] != 'H' ? Visibility.Visible : Visibility.Collapsed;
                drawingFileText.Text = Path.GetFileName(openFileDialog.FileName);
                EnableSubmit();
            }
        }

        private void productNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            productName = textBox.Text.ToUpper().Trim();
            SetValues();
        }

        private void customerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            customerName = textBox.Text.Trim();
            SetValues();
        }

        private void SetValues()
        {
            prodNameText.Text = productName;
            custNameText.Text = customerName;
            EnableSubmit();
        }

        private void EnableSubmit()
        {
            createProductButton.IsEnabled = !String.IsNullOrWhiteSpace(productName) &&
                                            !String.IsNullOrWhiteSpace(customerName) &&
                                            !String.IsNullOrWhiteSpace(filename);
        }
    }
}
