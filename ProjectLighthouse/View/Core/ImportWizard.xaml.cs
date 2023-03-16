using Microsoft.Win32;
using ProjectLighthouse.Model.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        void StoreColumns(string targetFilePath)
        {
            string[] lines = File.ReadAllLines(targetFilePath);
            fileContents = lines;

            if (lines.Length == 0)
            {
                MessageBox.Show("No data");
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
            if (!MappingIsValid())
            {
                MessageBox.Show("Mapping is not valid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SimulateImport();


        }

        private void SimulateImport()
        {
            if (SelectedImportType.Name is nameof(TurnedProduct))
            {

            }
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
