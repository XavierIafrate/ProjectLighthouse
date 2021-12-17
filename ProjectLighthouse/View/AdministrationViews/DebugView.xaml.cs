using Microsoft.Win32;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for DebugView.xaml
    /// </summary>
    public partial class DebugView : UserControl
    {
        TextBoxOutputter outputter;

        public DebugView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);

            Console.WriteLine("Updating Users");

            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                user.LastLoginText = user.LastLogin.ToString("s");
                DatabaseHelper.Update(user);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<MachineOperatingBlock> stats = DatabaseHelper.Read<MachineOperatingBlock>();
            CSVHelper.WriteListToCSV(stats, "Performance");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>();

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Browse CSV Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "CSV files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if ((bool)openFileDialog1.ShowDialog())
            {
                List<TurnedProduct> updates = CSVHelper.LoadSalesStatsFromCSV(openFileDialog1.FileName);

                foreach (TurnedProduct product in updates)
                {
                    TurnedProduct currentProduct = products.Find(x => x.ProductName == product.ProductName);

                    if (currentProduct != null)
                    {
                        currentProduct.QuantitySold = product.QuantitySold;
                        currentProduct.NumberOfOrders = product.NumberOfOrders;

                        DatabaseHelper.Update(currentProduct);
                    }
                }

            }
        }
    }
}
