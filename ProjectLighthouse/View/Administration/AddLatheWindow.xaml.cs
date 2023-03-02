using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddLatheWindow : Window
    {
        private List<Lathe> ExistingLathes;
        public Lathe Lathe { get; set; }
        public Lathe? originalLathe;

        public bool SaveExit;
        public AddLatheWindow(List<Lathe> existingLathes, Lathe? lathe = null)
        {
            InitializeComponent();

            ExistingLathes = existingLathes;

            if (lathe is not null)
            {
                originalLathe = lathe;
                Lathe = (Lathe)originalLathe.Clone();
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

            DataContext = this;
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
                Lathe.CreatedBy= App.CurrentUser.UserName;
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
    }
}
