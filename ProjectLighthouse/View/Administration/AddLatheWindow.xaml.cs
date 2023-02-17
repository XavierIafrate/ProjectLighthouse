using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
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
            }
            else
            {
                Lathe = new();
            }


            SetUiElements();

            DataContext = this;
        }

        private void SetUiElements()
        {
            bool editing = originalLathe is not null;

            Title = editing ? "Edit Lathe" : "Add Lathe";
            TitleText.Text = editing ? $"Editing '{Lathe.Id}'" : "New Lathe";
            SubmitButton.Tag = editing ? "Update" : "Create";
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Lathe.ValidateAll();

            if(Lathe.HasErrors)
            {
                return;
            }

            if (originalLathe is not null)
            {
                List<string> changes = originalLathe.GetChanges(Lathe);

                if (changes.Count == 0)
                {

                }
            }

        }


        private void AllowNumbersAndPeriodOnly(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textBox.Text, e);
        }
    }
}
