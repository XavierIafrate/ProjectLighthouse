using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Windows;
using System.Windows.Controls;

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
            InitializeComponent();
            viewModel = Resources["vm"] as RequestViewModel;
            updateNotes();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateRequest();
        }

        private void updateNotes()
        {
            Request request = (Request)requests_ListView.SelectedValue;
            if (notesTextBox == null)
            {
                notesTextBox = new RichTextBox();
            }
            notesTextBox.Document.Blocks.Clear();
            notesTextBox.AppendText(request.Notes);
        }


        private void requests_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateNotes();
        }
    }
}
