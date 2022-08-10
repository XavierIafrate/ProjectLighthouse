using ProjectLighthouse.Model;
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
using System.Windows.Shapes;

namespace ProjectLighthouse.View.HelperWindows
{
    /// <summary>
    /// Interaction logic for EditDeliveryNoteWindow.xaml
    /// </summary>
    public partial class EditDeliveryNoteWindow : Window
    {
        public EditDeliveryNoteWindow(DeliveryItem item)
        {
            InitializeComponent();
            this.Title += $" {item.Id:0}";
            productText.Text = item.Product;
            poText.Text = item.PurchaseOrderReference;
        }
    }
}
