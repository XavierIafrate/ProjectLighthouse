using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for AddSpecialPartWindow.xaml
    /// </summary>
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
            this.Close();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";

            string openDir = "H:\\Sales Office";
            if(!Directory.Exists(openDir))
            {
                openDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                if(filename[0] != 'H')
                {
                    fileWarning.Visibility = Visibility.Visible;
                } else
                {
                    fileWarning.Visibility = Visibility.Collapsed;
                }
                drawingFileText.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
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
