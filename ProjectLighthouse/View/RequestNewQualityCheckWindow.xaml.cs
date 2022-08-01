using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class RequestNewQualityCheckWindow : Window
    {

        public bool RequestAdded = false;

        public RequestNewQualityCheckWindow()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(productNameTextBox.Text.Trim()))
            {
                MessageBox.Show("Product Name cannot be blank.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(specDetailsTextBox.Text.Trim()) && string.IsNullOrWhiteSpace(FilePicker.FilePath))
            {
                MessageBox.Show("You must provide a specification through one of the two methods.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            QualityCheck newCheck = new()
            {
                Product = productNameTextBox.Text.Trim().ToUpperInvariant(),
                RaisedAt = System.DateTime.Now,
                RaisedBy = App.CurrentUser.UserName,
                SpecificationDetails = specDetailsTextBox.Text.Trim(),
                SpecificationDocument = FilePicker.FilePath,
                RequiredBy = (System.DateTime)RequiredDate.SelectedDate,
            };

            if (DatabaseHelper.Insert(newCheck))
            {
                this.RequestAdded = true;
                Close();
            }
        }
    }
}
