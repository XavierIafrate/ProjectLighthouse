using ProjectLighthouse.Model;
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
        private NewRequestViewModel viewModel;
        public NewRequestView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as NewRequestViewModel;
            EnableSubmit();
        }

        private void EnableSubmit()
        {
            submitButton.IsEnabled = DateRequiredCalendarView.SelectedDate.HasValue &&
                                     viewModel.NewRequest.QuantityRequired > 0 &&
                                     productsListBox.SelectedItem != null &&
                                     App.CurrentUser.CanRaiseRequest;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SubmitRequest())
            {
                quantityBox.Text = "";
                date_display.Text = "";
                notesTextBox.Document.Blocks.Clear();
            }
        }

        private void DateRequiredCalendarView_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateRequiredCalendarView.SelectedDate == null)
            {
                return;
            }

            date_display.Text = DateRequiredCalendarView.SelectedDate.Value.ToString("dd/MM/yyyy");
            DateGhost.Visibility = string.IsNullOrEmpty(date_display.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
            viewModel.NewRequest.DateRequired = DateRequiredCalendarView.SelectedDate.Value;
            EnableSubmit();
        }

        private void QuantityBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (string.IsNullOrEmpty(textbox.Text))
            {
                QuantityGhost.Visibility = Visibility.Visible;
                return;
            }

            QuantityGhost.Visibility = Visibility.Hidden;

            if (int.TryParse(textbox.Text, out int j))
            {
                if (j > 0)
                {
                    viewModel.NewRequest.QuantityRequired = j;
                    viewModel.CalculateInsights();
                }
            }
            EnableSubmit();
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (viewModel.SelectedGroup == "Live")
            {
                if (productsListBox.SelectedValue is TurnedProduct turnedProduct)
                {
                    int NetStockLevel = turnedProduct.QuantityInStock + turnedProduct.QuantityOnPO - turnedProduct.QuantityOnSO;
                    quantityBox.Text = Math.Abs(NetStockLevel).ToString();
                }
            }

            EnableSubmit();
        }

        private void NotesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new(notesTextBox.Document.ContentStart, notesTextBox.Document.ContentEnd);
            if (viewModel != null && textRange.Text.Length >= 2)
            {
                viewModel.NewRequest.Notes = textRange.Text[0..^2];
                NotesGhost.Visibility = string.IsNullOrEmpty(textRange.Text[0..^2])
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void QuantityBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
