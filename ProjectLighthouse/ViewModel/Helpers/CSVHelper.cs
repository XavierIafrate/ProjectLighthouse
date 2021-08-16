using CsvHelper;
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
        public static void WriteListToCSV<T>(List<T> stuff, string filePrefix)
        {
            string filename = string.Format("{0}_{1:ddMMyy_HHmmss}.csv", filePrefix, DateTime.Now);
            filename = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            using (StreamWriter writer = new(filename))
            using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
            {
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



    }
}
