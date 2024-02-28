using HarfBuzzSharp;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddLatheWindow : Window, INotifyPropertyChanged
    {
        private List<Lathe> ExistingLathes;
        public Lathe Lathe { get; set; }
        public Lathe? originalLathe;

        public bool SaveExit;

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

        private List<string> baseFeatures;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AddLatheWindow(List<Lathe> existingLathes, Lathe? lathe = null)
        {
            InitializeComponent();

            ExistingLathes = existingLathes;

            if (lathe is not null)
            {
                originalLathe = lathe;
                Lathe = (Lathe)originalLathe.Clone();
                Lathe.ValidateAll();
                IdTextBox.IsEnabled = false;
                Title = "Edit Lathe";
                AddButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpdateButton.Visibility = Visibility.Collapsed;
                Title = "Add Lathe";
                Lathe = new();
            }

           List<string> featureList = new();

            foreach (Lathe l in ExistingLathes)
            {
                if (lathe is not null)
                {
                    if (l.Id == lathe.Id)
                    {
                        continue;
                    }
                }

                featureList.AddRange(l.FeatureList);
            }

            featureList = featureList.Distinct().ToList();

            if (lathe is not null)
            {
                featureList = featureList.Where(f => !lathe.FeatureList.Contains(f)).ToList();
            }

            baseFeatures = featureList;
            ExistingFeatures = featureList;

            DataContext = this;
        }

        private void RemoveFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;
            List<string> updatedList = Lathe.FeatureList;
            updatedList.Remove(feature);

            Lathe.FeatureList = updatedList.ToList();
            ExistingFeatures = baseFeatures.Where(x => !Lathe.FeatureList.Contains(x)).ToList();
        }

        private void AddExistingFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not string feature) return;

            List<string> updatedList = Lathe.FeatureList;
            updatedList.Add(feature);


            Lathe.FeatureList = updatedList.ToList();

            ExistingFeatures = baseFeatures.Where(x => !Lathe.FeatureList.Contains(x)).ToList();
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Lathe.ValidateAll();

            if (originalLathe is not null)
            {
                originalLathe.ValidateAll();

                // Prevent new data errors
                if (originalLathe.NoErrors && Lathe.HasErrors)
                {
                    return;
                }

                try
                {
                    UpdateLathe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (Lathe.HasErrors)
                {
                    return;
                }

                try
                {
                    CreateLathe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            SaveExit = true;
            Close();

        }


        void UpdateLathe()
        {
            try
            {
                Lathe.CreatedBy = App.CurrentUser.UserName;
                Lathe.CreatedAt = DateTime.Now;
                DatabaseHelper.Update(Lathe, throwErrs: true);
                originalLathe = Lathe;
            }
            catch
            {
                throw;
            }
        }

        void CreateLathe()
        {
            try
            {
                DatabaseHelper.Insert(Lathe, throwErrs: true);
            }
            catch
            {
                throw;
            }
        }


        private void AllowNumbersAndPeriodOnly(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textBox.Text, e);
        }

        private void AddFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            string newFeature = NewFeatureTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newFeature)) return;

            if (Lathe.FeatureList.Contains(newFeature))
            {
                MessageBox.Show("Lathe already has that feature defined");
                return;
            }

            List<string> updatedList = Lathe.FeatureList;
            updatedList.Add(newFeature);


            Lathe.FeatureList = updatedList.ToList();

            NewFeatureTextBox.Clear();
            ExistingFeatures = baseFeatures.Where(x => !Lathe.FeatureList.Contains(x)).ToList();

        }

        private void NewFeatureTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddFeatureButton.IsEnabled = !string.IsNullOrWhiteSpace(NewFeatureTextBox.Text);
        }
    }
}
