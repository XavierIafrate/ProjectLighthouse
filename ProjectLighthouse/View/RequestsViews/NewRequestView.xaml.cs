using ProjectLighthouse.ViewModel;
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
                                     Int32.TryParse(quantityBox.Text, out _) &&
                                     productsListBox.SelectedItem != null;
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
            viewModel.newRequest.DateRequired = DateRequiredCalendarView.SelectedDate.Value;
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
                if (j > 0)
                {
                    viewModel.newRequest.QuantityRequired = j;
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
            TextRange textRange = new TextRange(notesTextBox.Document.ContentStart, notesTextBox.Document.ContentEnd);
            if (viewModel != null && textRange.Text.Length >= 2)
            {
                viewModel.newRequest.Notes = textRange.Text[0..^2];
            }
        }
    }
}
