using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ProjectLighthouse.View
{
    public partial class NewRequestView : UserControl
    {
        private NewRequestViewModel viewModel;
        public NewRequestView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as NewRequestViewModel;
            EnableSubmit();

            requiredDatePicker.FirstDayOfWeek = DayOfWeek.Monday;

        }

        private void EnableSubmit()
        {
            bool productSelected = (TurnedProduct)ProductsListBox.SelectedValue is not null;
            bool quantityOk = false;
            if(int.TryParse(quantityTextBox.Text, out int qty))
            {
                if(qty > 0 && qty < 200000)
                {
                    quantityOk = true;
                }
            }

            bool dateOk = requiredDatePicker.SelectedDate is not null;

            SubmitButton.IsEnabled = productSelected && quantityOk && dateOk;
        }

        private void DateRequiredCalendarView_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker control = sender as DatePicker;
            if (viewModel != null)
            {
                viewModel.NewRequest.DateRequired = control.SelectedDate.Value;
            }
            EnableSubmit();
        }

        private void QuantityBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out int j))
            {
                if (j > 0)
                {
                    viewModel.NewRequest.QuantityRequired = j;
                }
            }
            EnableSubmit();
        }

        private void NotesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TextRange textRange = new(notesTextBox.Document.ContentStart, notesTextBox.Document.ContentEnd);
            //if (viewModel != null && textRange.Text.Length >= 2)
            //{
            //    viewModel.NewRequest.Notes = textRange.Text[0..^2];
            //    NotesGhost.Visibility = string.IsNullOrEmpty(textRange.Text[0..^2])
            //        ? Visibility.Visible
            //        : Visibility.Hidden;
            //}
        }

        private void QuantityBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableSubmit();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSubmit();
            if (App.MainViewModel.SelectedViewModel is NewRequestViewModel vm)
            {
                vm.GetRecommendedManifest();
            }
        }

        private void requiredDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableSubmit();
        }
    }
}
