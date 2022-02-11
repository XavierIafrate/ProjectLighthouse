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
        }

        private void EnableSubmit()
        {
            if (viewModel != null)
            {
                submitButton.IsEnabled = DateRequiredCalendarView.SelectedDate > DateTime.Now &&
                    DateRequiredCalendarView.SelectedDate < DateTime.Now.AddMonths(13) &&
                        viewModel.NewRequest.QuantityRequired > 0 &&
                        productsListBox.SelectedItem != null &&
                        App.CurrentUser.CanRaiseRequest;
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SubmitRequest())
            {
                quantityBox.Text = "";
                notesTextBox.Document.Blocks.Clear();
            }
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
                    viewModel.CalculateInsights();
                }
            }
            EnableSubmit();
        }

        private void ProductsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateQuantityPrefill();
        }

        void PopulateQuantityPrefill()
        {
            if (viewModel == null)
            {
                return;
            }

            if (viewModel.SelectedGroup == "Live")
            {
                if (productsListBox.SelectedValue is TurnedProduct turnedProduct)
                {
                    quantityBox.Text = Math.Abs(turnedProduct.FreeStock()).ToString();
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateQuantityPrefill();
        }
    }
}
