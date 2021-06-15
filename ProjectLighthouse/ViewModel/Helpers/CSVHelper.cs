using CsvHelper;
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
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(stuff);
            }
            MessageBox.Show(String.Format("Saved to {0}", filename), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OpenWithDefaultProgram(filename);
        }

        public static void OpenWithDefaultProgram(string path)
        {
            Process fileopener = new();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }
    }
}
