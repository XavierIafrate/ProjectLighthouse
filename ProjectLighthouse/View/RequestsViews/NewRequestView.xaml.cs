using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for NewRequestView.xaml
    /// </summary>
    public partial class NewRequestView : UserControl
    {
        NewRequestViewModel viewModel;
        public NewRequestView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as NewRequestViewModel;
            enableSubmit();
        }

        private void enableSubmit()
        {
            submitButton.IsEnabled = DateRequiredCalendarView.SelectedDate.HasValue &&
                                     int.TryParse(quantityBox.Text, out _) &&
                                     productsListBox.SelectedItem != null &&
                                     App.CurrentUser.CanRaiseRequest;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SubmitRequest())
            {
                quantityBox.Text = "";
                notesTextBox.Document.Blocks.Clear();
            }
        }

        private void DateRequiredCalendarView_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateRequiredCalendarView.SelectedDate == null)
                return;

            viewModel.NewRequest.DateRequired = DateRequiredCalendarView.SelectedDate.Value;
            enableSubmit();
        }

        private void quantityBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (String.IsNullOrEmpty(textbox.Text))
            {
                return;
            }
            if (Int32.TryParse(textbox.Text, out int j))
            {
                if (j > 0 && j <= 100000)
                {
                    viewModel.NewRequest.QuantityRequired = j;
                    viewModel.CalculateInsights();
                }
                else
                {
                    MessageBox.Show("Invalid Quantity", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid Quantity", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            enableSubmit();
        }

        private void productsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            enableSubmit();
        }

        private void notesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new(notesTextBox.Document.ContentStart, notesTextBox.Document.ContentEnd);
            if (viewModel != null && textRange.Text.Length >= 2)
            {
                viewModel.NewRequest.Notes = textRange.Text[0..^2];
            }
        }

        private void quantityBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (graphGrid.Visibility == Visibility.Visible)
            {
                if (mainGrid.ColumnDefinitions[2].ActualWidth < 380)
                {
                    graphGrid.Visibility = Visibility.Collapsed;
                    mainGrid.ColumnDefinitions[2].Width = new(0);
                    mainGrid.ColumnDefinitions[1].Width = new(1, GridUnitType.Star);

                }
            }
            else
            {
                if (mainGrid.ColumnDefinitions[1].ActualWidth > (380 + 350))
                {
                    graphGrid.Visibility = Visibility.Visible;
                    mainGrid.ColumnDefinitions[2].Width = new(1, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = GridLength.Auto;
                }
            }
        }
    }
}
