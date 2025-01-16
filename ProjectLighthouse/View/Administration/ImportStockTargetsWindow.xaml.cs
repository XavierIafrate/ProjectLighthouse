using Microsoft.Win32;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ProjectLighthouse.View.Administration
{
    public partial class ImportStockTargetsWindow : Window
    {
        public ImportStockTargetsWindow()
        {
            InitializeComponent();
            existingProducts = DatabaseHelper.Read<TurnedProduct>().Select(x => x.ProductName).ToList();
        }

        string? filePath;
        Dictionary<string, int> updateList = new();
        List<string> existingProducts;

        bool setOthersToZero;

        void PickFile()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Text Files(*.txt) | *.txt"
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() ?? false)
            {
                filePath = openFileDialog.FileName;
                FileNameText.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                LoadData();
            }
        }

        void LoadData()
        {
            if(filePath == null)
            {
                MessageBox.Show("No File selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File does not exist:{Environment.NewLine}{filePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string[] fileContents;

            try
            {
                fileContents = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lighthouse encountered an error reading the file:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            updateList.Clear();

            foreach(string val in fileContents)
            {
                string[] line = val.Split(',');
                if(line.Length != 2)
                {
                    MessageBoxResult res = MessageBox.Show($"Failed to parse line:{Environment.NewLine}'{val}'{Environment.NewLine}Expected two data points, got {line.Length}. Skipping over or Cancel to stop.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    if(res == MessageBoxResult.Cancel) break;
                    continue;
                }

                if (!existingProducts.Contains(line[0]))
                {
                    MessageBoxResult res = MessageBox.Show($"Part does not exist in Lighthouse:{Environment.NewLine}'{line[0]}'{Environment.NewLine}Skipping over or Cancel to stop.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                    if (res == MessageBoxResult.Cancel) break;
                    continue;
                }

                if (!int.TryParse(line[1], out int target))
                {
                    MessageBoxResult res = MessageBox.Show($"Failed to parse '{line[1]}' as integer{Environment.NewLine}Skipping over or Cancel to stop.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Cancel) break;
                    continue;
                }

                if (target < 0)
                {
                    MessageBoxResult res = MessageBox.Show($"Target for {line[0]} is less than zero so will not be added.{Environment.NewLine}Skipping over or Cancel to stop.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Cancel) break;
                    continue;
                }

                if (target > 100_000)
                {
                    MessageBoxResult res = MessageBox.Show($"Target for {line[0]} is over 100,000 which is max limit, so will not be added.{Environment.NewLine}Skipping over or Cancel to stop.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (res == MessageBoxResult.Cancel) break;
                    continue;
                }

                if (updateList.ContainsKey(line[0]))
                {
                    MessageBox.Show($"Value for item '{line[0]}' occurs more than once{Environment.NewLine}Value will be overwritten.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    updateList[line[0]] = target;
                }
                else
                {
                    updateList.Add(line[0], target);
                }
            }

            DataGrid.ItemsSource = null;
            DataGrid.ItemsSource = updateList;
        }

        private void PickFileButton_Click(object sender, RoutedEventArgs e)
        {
            PickFile();
        }

        private void ReloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            setOthersToZero = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            setOthersToZero = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            int numUpdates = updateList.Count;



            if (setOthersToZero)
            {
                int numSetToZero = existingProducts.Count - numUpdates;
                MessageBoxResult res = MessageBox.Show($"Executing this will set target stock to zero for {numSetToZero:#,##0} items, and update the values for {numUpdates:#,##0} items.{Environment.NewLine}{Environment.NewLine}Would you like to continue?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;

                res = MessageBox.Show($"Are you sure?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;
                res = MessageBox.Show($"Are you super duper sure?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;
                res = MessageBox.Show($"Just double checking you mean to click those yes buttons?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;
            }
            else 
            {
                if(numUpdates == 0)
                {
                    MessageBox.Show("No items loaded to update");
                    return;
                }
                MessageBoxResult res = MessageBox.Show($"Executing this will update the values for {numUpdates:#,##0} items.{Environment.NewLine}{Environment.NewLine}Would you like to continue?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;

                res = MessageBox.Show($"Are you sure?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;
                res = MessageBox.Show($"Are you super duper sure?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;
                res = MessageBox.Show($"Just double checking you mean to click those yes buttons?", "Confirm action", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes) return;

            }
            bool result;
            try
            {
                result = RunUpdate();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to process update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (result) 
            {
                MessageBox.Show("Success");
            }
            else
            {
                MessageBox.Show("Failed - no error message came through :(");
            }
        }

        private bool RunUpdate()
        {
            using SQLiteConnection conn = DatabaseHelper.GetConnection();
            conn.BeginTransaction();

            if (setOthersToZero)
            {
                try
                {
                    conn.Execute($"UPDATE {nameof(TurnedProduct)} SET QuantitySold = 0");
                }
                catch (Exception ex) 
                {
                    conn.Rollback();
                    conn.Close();
                    throw new Exception($"Error when setting all items QuantitySold to zero: {ex.Message}");
                }
            }

            foreach((string sku, int target) in updateList)
            {
                try
                {
                    conn.Execute($"UPDATE {nameof(TurnedProduct)} SET QuantitySold = {target:0} WHERE ProductName='{sku}'");
                }
                catch (Exception ex)
                {
                    conn.Rollback();
                    conn.Close();
                    throw new Exception($"Error when updating {sku} QuantitySold to {target}: {ex.Message}");
                }
            }

            conn.Commit();
            conn.Close();

            return true;
        }
    }
}
