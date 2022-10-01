using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Requests
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
            if (int.TryParse(quantityTextBox.Text, out int qty))
            {
                if (qty > 0 && qty < 200000)
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

        private void QuantityBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine($"Height: '{ActualHeight:N2}' Width: '{ActualWidth:N2}'");

            //remove col below 1150
            analyticsCol.Width = ActualWidth < 1150
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);

            // remove lead times below 710
            LeadTimesGrid.Visibility = ActualHeight < 710
                ? Visibility.Collapsed
                : Visibility.Visible;
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
