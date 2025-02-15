﻿using CsvHelper;
using CsvHelper.TypeConversion;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Quality;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
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

        public static void WriteListToCSV<T>(List<T> stuff, string filePrefix, string dateFormat = "s")
        {
            string filename = $"{filePrefix}_{DateTime.Now:ddMMyy_HHmmss}.csv";
            filename = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);

            using (StreamWriter writer = new(filename, false, Encoding.UTF8))
            using (CsvWriter csv = new(writer, CultureInfo.InvariantCulture))
            {
                TypeConverterOptions dateOptions = new() { Formats = new[] { dateFormat } };
                TypeConverterOptions doubleOptions = new() { Formats = new[] { "0.00000" } };
                csv.Context.TypeConverterOptionsCache.AddOptions<DateTime>(dateOptions);
                csv.Context.TypeConverterOptionsCache.AddOptions <double>(doubleOptions);
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
                        IsReject = csv.GetField<bool>("IsReject"),
                        IsDelivered = csv.GetField<bool>("IsDelivered"),
                        MaterialBatch = csv.GetField("MaterialBatch"),
                    };

   

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

        public static List<CalibrationCertificate> LoadCertificatesFromCSV(string path)
        {
            List<CalibrationCertificate> records = new();

            try
            {
                using StreamReader reader = new(path);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    DateTime lastCal = DateTime.MinValue;
                    if (DateTime.TryParse(csv.GetField<string>("DateIssued"), out DateTime tmpDate))
                    {
                        lastCal = tmpDate;
                    }

                    CalibrationCertificate record = new()
                    {
                        Id = csv.GetField<int>("Id"),
                        Instrument = csv.GetField<string>("Instrument"),
                        UKAS = csv.GetField<bool>("UKAS"),
                        CalibrationHouse = csv.GetField<string>("CalibrationHouse"),
                        CertificateNumber = csv.GetField<string>("CertificateNumber"),
                        DateIssued = lastCal,
                        IsPass = csv.GetField<bool>("IsPass"),
                        Url = csv.GetField<string>("Url"),
                        AddedAt = DateTime.Now,
                        AddedBy = "system"
                    };

                    records.Add(record);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return records;
        }

        public static List<CalibratedEquipment> LoadInstrumentsFromCSV(string path)
        {
            List<CalibratedEquipment> records = new();

            try
            {
                using StreamReader reader = new(path);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    DateTime lastCal = DateTime.MinValue;
                    if (DateTime.TryParse(csv.GetField<string>("LastCalibrated"), out DateTime tmpDate))
                    {
                        lastCal = tmpDate;
                    }

                    int calInterval = 0;
                    if (int.TryParse(csv.GetField<string>("LastCalibrated"), out int tmp))
                    {
                        calInterval = tmp;
                    }

                    CalibratedEquipment record = new()
                    {
                        Id = csv.GetField<int>("Id"),
                        EquipmentId = csv.GetField<string>("EquipmentId"),
                        Make = csv.GetField<string>("Make"),
                        Model = csv.GetField<string>("Model"),
                        SerialNumber = csv.GetField<string>("SerialNumber"),
                        Type = csv.GetField<string>("Type"),
                        Location = csv.GetField<string>("Location"),
                        EnteredSystem = DateTime.Now,
                        LastCalibrated = lastCal,
                        CalibrationIntervalMonths = calInterval,
                        UKAS = csv.GetField<bool>("UKAS"),
                        AddedBy = "system",
                        RequiresCalibration = csv.GetField<bool>("RequiresCalibration"),
                        IsOutOfService = csv.GetField<bool>("IsOutOfService"),
                    };

                    records.Add(record);
                }
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return records;
        }
    }
}
