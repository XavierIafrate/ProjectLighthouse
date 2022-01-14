using CsvHelper;
using CsvHelper.TypeConversion;
using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class CSVHelper
    {

        public static void WriteCsv<T>(List<T> list, string destinationDir, string fileNameNoExt)
        {
            string filename = Path.Join(destinationDir, $"{fileNameNoExt}.csv");

            using (StreamWriter writer = new(filename))
            using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
            {
                TypeConverterOptions options = new() { Formats = new[] { "s" } };
                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                csv.WriteRecords(list);
            }

            ConsoleColor initialColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Written to '{filename}'\n\t\t{typeof(T).FullName}\n\t\t{list.Count:#,##0} records.\n");
            Console.ForegroundColor = initialColour;
        }

        public static void WriteListToCSV<T>(List<T> stuff, string filePrefix)
        {
            string filename = $"{filePrefix}_{DateTime.Now:ddMMyy_HHmmss}.csv";
            filename = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);

            using (StreamWriter writer = new(filename))
            using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
            {
                TypeConverterOptions options = new TypeConverterOptions { Formats = new[] { "s" } };
                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                csv.WriteRecords(stuff);
            }
            MessageBox.Show($"Saved to {filename}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OpenWithDefaultProgram(filename);
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        public static List<Lot> GetLotsFromCSV(string path)
        {
            List<Lot> Lots = new();

            try
            {
                using StreamReader reader = new(path);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    Lot record = new()
                    {
                        ProductName = csv.GetField("ProductName"),
                        Order = csv.GetField("Order"),
                        AddedBy = csv.GetField("AddedBy"),
                        Quantity = csv.GetField<int>("Quantity"),
                        ExcelDate = csv.GetField("ExcelDate"),
                        IsReject = csv.GetField<bool>("IsReject"),
                        IsDelivered = csv.GetField<bool>("IsDelivered"),
                        MaterialBatch = csv.GetField("MaterialBatch"),
                    };

                    if (DateTime.TryParse(record.ExcelDate, out DateTime _date))
                    {
                        record.Date = _date;
                    }
                    else
                    {
                        Debug.WriteLine($"Could not parse datetime: {record.ExcelDate}");
                    }

                    Lots.Add(record);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Lots;
        }

        public static List<TurnedProduct> LoadSalesStatsFromCSV(string path)
        {
            List<TurnedProduct> products = new();

            try
            {
                using StreamReader reader = new(path);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    TurnedProduct record = new()
                    {
                        ProductName = csv.GetField<string>("ProductName"),
                        QuantitySold = csv.GetField<int>("QuantitySold"),
                        NumberOfOrders = csv.GetField<int>("NumberOfOrders"),
                        
                    };

                    products.Add(record);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return products;
        }
    }
}
