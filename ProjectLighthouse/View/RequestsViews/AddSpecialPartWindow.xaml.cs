using Microsoft.Win32;
using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View
{
    public partial class AddSpecialPartWindow : Window
    {
        public TurnedProduct NewProduct;
        public bool SaveExit;
        private string targetDrawing = "";
        private string destinationDrawing = "";
        private string drawingFile = "";

        public AddSpecialPartWindow()
        {
            InitializeComponent();
            NewProduct = new();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "PDF Files (*.pdf)|*.pdf"
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() == true)
            {
                string newDrawingPath = @"Drawings\Specials\" + Path.GetFileName(openFileDialog.FileName);

                string fullDrawingPath = Path.Join(App.ROOT_PATH, newDrawingPath);
                if (File.Exists(fullDrawingPath))
                {
                    MessageBox.Show($"A Document called '{Path.GetFileName(openFileDialog.FileName)}' already exists in the library.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    try
                    {
                        //File.Copy(openFileDialog.FileName, fullDrawingPath);
                        drawingFile = newDrawingPath;
                        destinationDrawing = fullDrawingPath;
                        targetDrawing = openFileDialog.FileName;
                        DrawingFile.Text = Path.GetFileName(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                    }
                }
            }
        }

        private bool ValidateFields()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(productNameTextBox.Text))
            {
                valid = false;
                MarkControl(productNameTextBox, valid: false);
            }
            else
            {
                MarkControl(productNameTextBox, valid: true);
            }

            if (string.IsNullOrEmpty(materialComboBox.Text))
            {
                valid = false;
                MarkControl(materialComboBox, valid: false);
            }
            else
            {
                MarkControl(materialComboBox, valid: true);
            }

            if (string.IsNullOrEmpty(threadSizeComboBox.Text))
            {
                valid = false;
                MarkControl(threadSizeComboBox, valid: false);
            }
            else
            {
                MarkControl(threadSizeComboBox, valid: true);
            }

            if ((bool)hex.IsChecked)
            {
                MarkControl(TorxSize, valid: true);

                if (string.IsNullOrEmpty(HexSize.Text))
                {
                    valid = false;
                    MarkControl(HexSize, valid: false);
                }
                else
                {
                    MarkControl(HexSize, valid: true);
                }
            }
            else if ((bool)torx.IsChecked)
            {
                MarkControl(HexSize, valid: true);

                if (string.IsNullOrEmpty(TorxSize.Text))
                {
                    valid = false;
                    MarkControl(TorxSize, valid: false);
                }
                else
                {
                    MarkControl(TorxSize, valid: true);
                }
            }
            else
            {
                MarkControl(HexSize, valid: true);
                MarkControl(TorxSize, valid: true);
            }

            if (string.IsNullOrEmpty(customerNameTextBox.Text))
            {
                valid = false;
                MarkControl(customerNameTextBox, valid: false);
            }
            else
            {
                MarkControl(customerNameTextBox, valid: true);
            }

            if (string.IsNullOrEmpty(majorDiameter.Text))
            {
                MarkControl(majorDiameter, valid: false);
                valid = false;
            }
            else
            {
                if (CheckTextIsFullDouble(majorDiameter.Text))
                {
                    if (double.Parse(majorDiameter.Text) <= TurnedProduct.MaxDiameter && double.Parse(majorDiameter.Text) > 0)
                    {
                        MarkControl(majorDiameter, valid: true);
                    }
                    else
                    {
                        MarkControl(majorDiameter, valid: false);
                        valid = false;
                    }
                }
                else
                {
                    MarkControl(majorDiameter, valid: false);
                    valid = false;
                }
            }

            if (string.IsNullOrEmpty(majorLength.Text))
            {
                MarkControl(majorLength, valid: false);
                valid = false;
            }
            else
            {
                if (CheckTextIsFullDouble(majorLength.Text))
                {
                    if (double.Parse(majorLength.Text) <= TurnedProduct.MaxLength && double.Parse(majorLength.Text) > 0)
                    {
                        MarkControl(majorLength, valid: true);
                    }
                    else
                    {
                        MarkControl(majorLength, valid: false);
                        valid = false;
                    }
                }
                else
                {
                    MarkControl(majorLength, valid: false);
                    valid = false;
                }
            }

            if (DrawingFile.Text == "<pdf_drawing>")
            {
                DrawingFile.Foreground = Brushes.Red;
                valid = false;
            }
            else
            {
                DrawingFile.Foreground = Brushes.Black;
            }

            return valid;
        }

        private bool CheckTextIsFullDouble(string text)
        {
            bool valid = false;
            
            if (text.Length > 3)
            {
                if (double.TryParse(text, out double val))
                {
                    if (text[^3] == '.')
                    {
                        valid = true;
                    }
                    else
                    {
                        MessageBox.Show($"It looks like '{text}' is not to two decimal places.");
                    }
                }
            }

            return valid;
        }

        private void MarkControl(Control control, bool valid)
        {
            BrushConverter converter = new();
            control.BorderBrush = valid
                ? (Brush)converter.ConvertFromString("#f0f0f0")
                : Brushes.Red;
        }

        private bool ImprintData()
        {
            ComboBoxItem selectedMaterial = materialComboBox.SelectedItem as ComboBoxItem;
            //ComboBoxItem selectedThread = threadSizeComboBox.SelectedItem as ComboBoxItem;
            ComboBoxItem selectedDriveSize = (bool)hex.IsChecked
                ? HexSize.SelectedItem as ComboBoxItem
                : (bool)torx.IsChecked
                        ? TorxSize.SelectedItem as ComboBoxItem
                        : null;

            try
            {
                NewProduct = new()
                {
                    AddedBy = App.CurrentUser.UserName,
                    AddedDate = DateTime.Now,
                    isSpecialPart = true,
                    DrawingFilePath = drawingFile,
                    ProductName = productNameTextBox.Text,
                    CustomerRef = customerNameTextBox.Text,
                    Material = selectedMaterial.Tag.ToString(),
                    BarID = GetBarSizeFromDiameter(double.Parse(majorDiameter.Text), selectedMaterial.Tag.ToString(), "round"),
                    MajorLength = double.Parse(majorLength.Text),
                    MajorDiameter = double.Parse(majorDiameter.Text),
                    ThreadSize = threadSizeComboBox.Text,
                    DriveType = (bool)hex.IsChecked ? "hex" : (bool)torx.IsChecked ? "torx" : "thumb",
                    DriveSize = selectedDriveSize.Tag.ToString() ?? Math.Ceiling(double.Parse(majorDiameter.Text)).ToString("0"),
                    ProductGroup = productNameTextBox.Text[..9]
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return false;
            }

            try
            {
                File.Copy(targetDrawing, destinationDrawing);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                return false;
            }

            return true;
        }

        private string GetBarSizeFromDiameter(double diameter, string material, string profile)
        {
            List<BarStock> bars = DatabaseHelper.Read<BarStock>();

            bars = bars.Where(b => (double)b.Size >= diameter && material == b.Material && profile == b.Form).OrderBy(b => b.Size).ToList();


            if (bars.Count > 0)
            {
                return bars.First().Id;
            }
            else
            {
                throw new InvalidOperationException("Could not find a suitable bar for this product. Contact the administrator."); ;
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                return;
            }

            if (ImprintData())
            {
                SaveExit = true;
                Close();
            }
        }

        private void ValidateDouble(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textbox.Text, e);
        }
    }
}
