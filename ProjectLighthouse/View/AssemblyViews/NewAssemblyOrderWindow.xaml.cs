using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ProjectLighthouse.View.AssemblyViews
{
    /// <summary>
    /// Interaction logic for NewAssemblyOrderWindow.xaml
    /// </summary>
    public partial class NewAssemblyOrderWindow : Window
    {
        private AssemblyManufactureOrder newOrder;
        public AssemblyManufactureOrder NewOrder
        {
            get { return newOrder; }
            set { newOrder = value; }
        }

        private int QuantityRequired { get; set; }

        public List<Drop> newDrops { get; set; }

        public List<AssemblyItem> Products { get; set; }

        public NewAssemblyOrderWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadData();
            singleRadio.IsChecked = true;
            byDayNumber.IsChecked = true;
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<AssemblyItem>().ToList();
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            bool isMulti = (bool)multiRadio.IsChecked;

            FlatRequiredDate.IsEnabled = !isMulti;
            CallOffDropQuantity.IsEnabled = isMulti;
            MultiDropStartingDate.IsEnabled = isMulti;
            nthDayOfMonthTextBox.IsEnabled = isMulti;
            nthComboBox.IsEnabled = isMulti;
            DayOfWeekComboBox.IsEnabled = isMulti;
            byDayNumber.IsEnabled = isMulti;
            byInterval.IsEnabled = isMulti;

            if (isMulti)
            {
                bool doByDayNumber = (bool)byDayNumber.IsChecked;
                nthDayOfMonthTextBox.IsEnabled = doByDayNumber;
                nthComboBox.IsEnabled = !doByDayNumber;
                DayOfWeekComboBox.IsEnabled = !doByDayNumber;
            }

        }

        //private void EnforceNumbers_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBox = sender as TextBox;

        //    if (string.IsNullOrEmpty(textBox.Text))
        //        return;

        //    if (int.TryParse(textBox.Text, out int i))
        //    {
        //        QuantityRequired = i;
        //    }

        //}

        private void CallOffDropQuantity_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) // space bar just has to be a precious little princess with it's own function
            {
                e.Handled = true;
                return;
            }

            string strKey = e.Key.ToString();
            if ((strKey.Contains("D") && strKey.Length == 2) || strKey.Contains("NumPad"))
            {
                if ("0123456789".Contains(strKey.Substring(strKey.Length - 1, 1)))
                {
                    e.Handled = false;
                    return;
                }
            }
            e.Handled = true;
            return;
        }
    }
}
