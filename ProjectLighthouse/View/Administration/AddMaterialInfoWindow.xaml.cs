using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddMaterialInfoWindow : Window, INotifyPropertyChanged
    {
        public MaterialInfo Material { get; set; }
        public MaterialInfo? originalMaterial;

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


        public bool SaveExit;


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddMaterialInfoWindow(List<string> existingMachineFeatures, MaterialInfo? material = null)
        {
            InitializeComponent();

            baseExistingFeaturesList = existingMachineFeatures;

            if (material is not null)
            {
                originalMaterial = material;
                Material = (MaterialInfo)material.Clone();
                Material.ValidateAll();

                ExistingFeatures = baseExistingFeaturesList.Where(x => !Material.RequiresFeaturesList.Contains(x)).ToList();

                CreateButton.Visibility = Visibility.Collapsed;
                Title = "Edit Material";
            }
            else
            {
                Material = new();
                ExistingFeatures = baseExistingFeaturesList.ToList();
                UpdateButton.Visibility = Visibility.Collapsed;
            }

            DataContext = this;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Material.ValidateAll();

            if (originalMaterial is not null)
            {
                originalMaterial.ValidateAll();
                // Prevent new data errors
                if (originalMaterial.NoErrors && Material.HasErrors)
                {
                    return;
                }

                try
                {
                    UpdateExistingMaterial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (Material.HasErrors)
                {
                    return;
                }

                try
                {
                    CreateMaterial();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            originalMaterial = Material;
            SaveExit = true;
            Close();
        }

        void UpdateExistingMaterial()
        {
            try
            {
                DatabaseHelper.Update(Material, throwErrs: true);
                originalMaterial = Material;
            }
            catch
            {
                throw;
            }
        }

        void CreateMaterial()
        {
            try
            {
                DatabaseHelper.Insert(Material, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }

        private void RemoveFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Material.RequiresFeaturesList = Material.RequiresFeaturesList.Where(x => x != feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Material.RequiresFeaturesList.Contains(x)).ToList();
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            Material.RequiresFeaturesList = Material.RequiresFeaturesList.Append(feature).ToList();
            ExistingFeatures = baseExistingFeaturesList.Where(x => !Material.RequiresFeaturesList.Contains(x)).ToList();
        }
    }
}
