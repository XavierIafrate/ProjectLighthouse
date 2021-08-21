using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for RequestView.xaml
    /// </summary>
    public partial class RequestView : UserControl
    {
        RequestViewModel viewModel;

        public RequestView()
        {
            viewModel = new RequestViewModel() { Window = this };
            DataContext = viewModel;
            InitializeComponent();
            
            //viewModel = Resources["vm"] as RequestViewModel;

            updateNotes();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateRequest();
        }

        private void updateNotes()
        {
            Request request = (Request)requests_ListView.SelectedValue;
            if (request == null)
                return;

            if (notesTextBox == null)
                notesTextBox = new RichTextBox();

            notesTextBox.Document.Blocks.Clear();
            notesTextBox.AppendText(request.Notes);
            if (date_display == null)
                return;

            date_display.Text = request.DateRequired.ToString("dd/MM/yyyy");
        }

        private void requests_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView.SelectedValue != null)
            {
                updateNotes();
            }
        }

        private void saveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // get qty
            TextBox textbox = quantityTextbox;
            if (string.IsNullOrEmpty(textbox.Text))
            {
                return;
            }
            if (int.TryParse(textbox.Text, out int j))
            {
                if (j < 0)
                    MessageBox.Show("Invalid Quantity", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Invalid Quantity", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // get notes
            TextRange textRange = new(notesTextBox.Document.ContentStart, notesTextBox.Document.ContentEnd);
            string notes = string.Empty;
            if (viewModel != null && textRange.Text.Length >= 2)
            {
                notes = textRange.Text[0..^2];
            }

            viewModel.UpdateRequirements(notes, j);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditDate_DatePicker.SelectedDate == null)
            {
                return;
            }

            if (date_display == null)
            {
                return;
            }

            date_display.Text = EditDate_DatePicker.SelectedDate.Value.ToString("dd/MM/yyyy");
            DateGhost.Visibility = string.IsNullOrEmpty(date_display.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void quantityTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (QuantityGhost == null)
                return;
            QuantityGhost.Visibility = string.IsNullOrEmpty(quantityTextbox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
