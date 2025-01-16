using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Administration
{
    public partial class NewBarWindow : Window, INotifyPropertyChanged
    {
        public BarStock NewBar { get; set; }
        public List<MaterialInfo> Materials { get; set; }
        public bool SaveExit { get; set; }

        private List<string> existingFeatures;
        public List<string> ExistingFeatures
        {
            get { return existingFeatures; }
            set
            {
                existingFeatures = value;
                OnPropertyChanged();
            }
        }

        private List<string> baseExistingFeaturesList;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NewBarWindow(List<string> existingMachineFeatures, BarStock? existingBar)
        {
            InitializeComponent();
            Materials = DatabaseHelper.Read<MaterialInfo>()
                .OrderBy(x => x.MaterialText)
                .ThenBy(x => x.GradeText)
                .ToList();
            OnPropertyChanged(nameof(Materials));

            baseExistingFeaturesList = existingMachineFeatures;

            if(existingBar is not null)
            {
                this.Title = "Edit Bar Stock";
                this.barIdTextBox.IsEnabled = false;
                ExistingFeatures = baseExistingFeaturesList.Where(x => !existingBar.RequiresFeaturesList.Contains(x)).ToList();
                NewBar = (BarStock)existingBar.Clone();
                AddBarButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                int defaultMaterialId = Materials.Count > 0 ? Materials[0].Id : 0;
                NewBar = new() { MaterialId = defaultMaterialId };
                ExistingFeatures = baseExistingFeaturesList.ToList();
                UpdateButton.Visibility = Visibility.Collapsed;
            }

            materialComboBox.SelectedIndex = Materials.IndexOf(Materials.Find(x => x.Id == NewBar?.MaterialId));
        }

        private void ValidateInt(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void ValidateDouble(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textbox.Text, e);
        }

        private void AddBarButton_Click(object sender, RoutedEventArgs e)
        {
            NewBar.ValidateAll();
            if (NewBar.HasErrors)
            {
                return;
            }

            if (!DatabaseHelper.Insert(NewBar))
            {
                MessageBox.Show("Failed to insert record to database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveExit = true;
            Close();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            NewBar.ValidateAll();
            if (NewBar.HasErrors)
            {
                return;
            }

            if (!DatabaseHelper.Update(NewBar))
            {
                MessageBox.Show("Failed to update record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveExit = true;
            Close();
        }

        private void RemoveFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;

            NewBar.RequiresFeaturesList = NewBar.RequiresFeaturesList.Where(x => x != feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !NewBar.RequiresFeaturesList.Contains(x)).ToList();
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;

            NewBar.RequiresFeaturesList = NewBar.RequiresFeaturesList.Append(feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !NewBar.RequiresFeaturesList.Contains(x)).ToList();
        }
    }
}
