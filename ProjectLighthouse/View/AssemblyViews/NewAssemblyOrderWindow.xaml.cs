using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
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

        private void CallOffDropQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            char key = (char)e.Key;

            if (!char.IsControl(key) && !char.IsDigit(key) &&
            (key != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((key == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}
