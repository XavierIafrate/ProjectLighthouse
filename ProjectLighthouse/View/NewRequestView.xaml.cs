using ProjectLighthouse.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                                     Int32.TryParse(quantityBox.Text, out int j) &&
                                     productsListBox.SelectedItem != null;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SubmitRequest();
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
    }
}
