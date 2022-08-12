using ProjectLighthouse.Model;
using System.Windows;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class EditDeliveryNoteWindow : Window
    {
        public EditDeliveryNoteWindow(DeliveryItem item)
        {
            InitializeComponent();
            this.Title += $" {item.Id:0}";
            productText.Text = item.Product;
            poText.Text = item.PurchaseOrderReference;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
