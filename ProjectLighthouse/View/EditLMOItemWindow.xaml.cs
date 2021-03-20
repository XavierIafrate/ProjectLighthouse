using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
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

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for EditLMOItemWindow.xaml
    /// </summary>
    public partial class EditLMOItemWindow : Window
    {
        public LatheManufactureOrderItem Item;

        public EditLMOItemWindow(LatheManufactureOrderItem item)
        {
            Item = item;
            InitializeComponent();

            this.DataContext = Item;

            QuantityMadeTextbox.Text = item.QuantityMade.ToString();
            QuantityRejectTextbox.Text = item.QuantityReject.ToString();
            PopulateCycleTimes();

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(CycleTime_Min.Text, out int min))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Int32.TryParse(CycleTime_Sec.Text, out int sec))
            {
                MessageBox.Show("Invalid cycle time entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int cycleTime = min * 60 + sec;
            Item.CycleTime = cycleTime;

            if (!Int32.TryParse(QuantityMadeTextbox.Text, out int qtyMade))
            {
                MessageBox.Show("Invalid entry to Quantity Made field.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Int32.TryParse(QuantityRejectTextbox.Text, out int qtyReject))
            {
                MessageBox.Show("Invalid entry to Quantity Rejected field", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Item.QuantityMade = qtyMade;
            Item.QuantityReject = qtyReject;
            Item.UpdatedAt = DateTime.Now;
            Item.UpdatedBy = App.currentUser.GetFullName();

            DatabaseHelper.Update(Item);

            //Update master cycle time record
            var product = DatabaseHelper.Read<TurnedProduct>().Where(n => n.ProductName == Item.ProductName).ToList();
            //should return only one!
            if (product != null)
            {
                TurnedProduct thisProduct = product.First();
                thisProduct.CycleTime = cycleTime;
                DatabaseHelper.Update(thisProduct);
            }
            else
            {
                MessageBox.Show("Failed to update product record.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Close();
        }

        private void PopulateCycleTimes()
        {
            intCycleTimeText.Text = String.Format("({0}s)", Item.CycleTime);
            int secs = Item.CycleTime % 60;
            int mins = (Item.CycleTime - secs) / 60;

            CycleTime_Min.Text = mins.ToString();
            CycleTime_Sec.Text = secs.ToString();

        }

        private void CalculateCycleTime()
        {
            var bc = new BrushConverter();

            if (Int32.TryParse(CycleTime_Min.Text, out int min))
            {
                CycleTime_Min.BorderBrush = (Brush)bc.ConvertFrom("#FFABADB3");
            }
            else
            {
                CycleTime_Min.BorderBrush = Brushes.Red;
                intCycleTimeText.Text = String.Format("({0}s)", "?");
                return;
            }

            if (Int32.TryParse(CycleTime_Sec.Text, out int sec))
            {
                CycleTime_Sec.BorderBrush = (Brush)bc.ConvertFrom("#FFABADB3");
            }
            else
            {
                CycleTime_Sec.BorderBrush = Brushes.Red;
                intCycleTimeText.Text = String.Format("({0}s)", "?");
                return;
            }

            int cycleTime = min * 60 + sec;
            intCycleTimeText.Text = String.Format("({0}s)", cycleTime);

        }

        private void CycleTime_Min_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }

        private void CycleTime_Sec_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateCycleTime();
        }
    }
}
