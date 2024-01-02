using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using static ProjectLighthouse.Model.BaseObject;

namespace ProjectLighthouse.View.Core
{
    public partial class ImportWizard : Window, INotifyPropertyChanged
    {
        public List<Type> ImportTypes { get; set; }

        private Type selectedImportType;
        public Type SelectedImportType
        {
            get { return selectedImportType; }
            set
            {
                selectedImportType = value;
                ConfigureMapping(selectedImportType);
                OnPropertyChanged();
            }
        }

        private string targetFilePath;

        public string TargetFilePath
        {
            get { return targetFilePath; }
            set
            {
                targetFilePath = value;
                StoreColumns(targetFilePath);
                OnPropertyChanged();
            }
        }

        public List<string> AvailableHeaders { get; set; } = new();


        string[] fileHeader;
        string[] fileContents;

        public List<ColumnMap> Mapping { get; set; } = new();


        public ImportWizard()
        {
            InitializeComponent();

            ImportTypes = new() { typeof(TurnedProduct) };
            if (ImportTypes.Count > 0)
            {
                SelectedImportType = ImportTypes.First();
            }

            DataContext = this;
        }

        private static string[] ReadAllLinesSafe(string path)
        {
            using FileStream csv = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader sr = new(csv);
            List<string> file = new();

            while (!sr.EndOfStream)
            {
                file.Add(sr.ReadLine());
            }

            return file.ToArray();
        }

        void StoreColumns(string targetFilePath)
        {
            string[] lines = ReadAllLinesSafe(targetFilePath);
            fileContents = lines;

            if (lines.Length == 0)
            {
                MessageBox.Show("No data");
                return;
            }

            if (lines.Length == 1)
            {
                MessageBox.Show("Only Header row is in file");
                return;
            }

            string header = lines[0];

            fileHeader = header.Split(',');

            UpdateAvailable();
        }

        void UpdateAvailable()
        {
            if (fileHeader is null)
            {
                AvailableHeaders = new();
                return;
            }

            AvailableHeaders = fileHeader.Where(x => !Mapping.Any(y => y.From == x)).ToList();
            OnPropertyChanged(nameof(AvailableHeaders));


            foreach (ColumnMap map in Mapping)
            {
                if (AvailableHeaders.Contains(map.Attribute.Name))
                {
                    map.From = map.Attribute.Name;
                }
            }

            Mapping = new(Mapping);
            OnPropertyChanged(nameof(Mapping));
        }


        void ConfigureMapping(Type selectedImportType)
        {
            Mapping.Clear();

            PropertyInfo[] properties = selectedImportType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Import? importAttribute = property.GetCustomAttribute<Import>();
                if (importAttribute is null)
                {
                    continue;
                }

                Mapping.Add(new(property, importAttribute, ""));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public class ColumnMap
        {
            public PropertyInfo To { get; set; }
            public Import Attribute { get; set; }
            public string From { get; set; }

            public ColumnMap(PropertyInfo to, Import att, string from)
            {
                From = from; Attribute = att; To = to;
            }
        }

        private void PickButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "CSV Files (*.csv)|*.csv"
            };

            string openDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.InitialDirectory = openDir;

            if (openFileDialog.ShowDialog() ?? false)
            {
                TargetFilePath = openFileDialog.FileName;
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!MappingIsValid())
            //{
            //    MessageBox.Show("Mapping is not valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            TestImport();

            //SimulateImport();


        }

        void TestImport()
        {
            List<TmpProductImport> records;
            try
            {
                using StreamReader reader = new(TargetFilePath);
                using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
                //csv.Context.RegisterClassMap<TurnedProductMap>();

                records = csv.GetRecords<TmpProductImport>().ToList();
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
                return;
            }

            TryImport(records);
        }

        void TryImport(List<TmpProductImport> records)
        {
            List<TurnedProduct> products = DatabaseHelper.Read<TurnedProduct>(throwErrs: true);


            foreach (TmpProductImport record in records)
            {
                TurnedProduct? existingRecord = products.Find(x => x.ProductName == record.ProductName);

                if (existingRecord is null)
                {
                    // New record
                    TurnedProduct newRecord = new()
                    {
                        ProductName = record.ProductName,
                        MajorLength = record.MajorLength,
                        MajorDiameter = record.MajorDiameter,
                        PartOffLength = record.PartOffLength,
                        MaterialId = record.MaterialId,
                        GroupId = record.GroupId,
                        AddedBy = App.CurrentUser.UserName,
                        AddedDate = DateTime.Now,
                    };

                    try
                    {
                        DatabaseHelper.Insert<TurnedProduct>(newRecord, throwErrs: true);
                    }
                    catch (Exception ex)
                    {
                        NotificationManager.NotifyHandledException(ex);
                        return;
                    }
                }
                else
                {

                    if (existingRecord.MajorLength != record.MajorLength)
                    {

                    }
                    existingRecord.MajorLength = record.MajorLength;
                    existingRecord.MajorDiameter = record.MajorDiameter;
                    existingRecord.PartOffLength = record.PartOffLength;
                    existingRecord.MaterialId = record.MaterialId;
                    existingRecord.GroupId = record.GroupId;

                    try
                    {
                        DatabaseHelper.Update<TurnedProduct>(existingRecord, throwErrs: true);
                    }
                    catch (Exception ex)
                    {
                        NotificationManager.NotifyHandledException(ex);
                        return;
                    }
                }
            }
        }

        public sealed class TurnedProductMap : ClassMap<TurnedProduct>
        {
            public TurnedProductMap()
            {

                Map(m => m.QuantityInStock).Default(0);

            }
        }

        private void SimulateImport()
        {
            //if (SelectedImportType.Name is nameof(TurnedProduct))
            //{
            //    using (var reader = new StreamReader("path\\to\\file.csv"))
            //    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            //    {
            //        //csv.Context.RegisterClassMap<TurnedProductMap>();
            //        IEnumerable<TurnedProduct> records = csv.GetRecords<TurnedProduct>();
            //    }
            //}
        }

        public class TmpProductImport
        {
            public string ProductName { get; set; }
            public double MajorLength { get; set; }
            public double MajorDiameter { get; set; }
            public double PartOffLength { get; set; }
            public int MaterialId { get; set; }
            public int GroupId { get; set; }
        }

        void ImportTurnedProduct(bool simulate)
        {
            //List<TurnedProduct> turnedProducts = DatabaseHelper.Read<TurnedProduct>();
            //SelectedImportType.
            //List<TurnedProduct> toImport = GetObjectsFromFile


        }


        private bool MappingIsValid()
        {
            if (SelectedImportType is null)
            {
                return false;
            }

            if (Mapping is null)
            {
                return false;
            }

            List<string> userConfigMapping = Mapping.Select(x => x.From).ToList();

            return userConfigMapping.All(x => !string.IsNullOrEmpty(x))
                && userConfigMapping.Distinct().Count() == userConfigMapping.Count;
        }
    }
}
