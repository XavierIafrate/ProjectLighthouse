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

            newDrops = new List<Drop>();
            QuantityRequired = 0;
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
            bool doByDayNumber = (bool)byDayNumber.IsChecked;
            if (isMulti)
            {
                nthDayOfMonthTextBox.IsEnabled = doByDayNumber;
                nthComboBox.IsEnabled = !doByDayNumber;
                DayOfWeekComboBox.IsEnabled = !doByDayNumber;
            }


        }

        private void CalculateDrops(bool multi, bool byDayNumber = false)
        {
            newDrops.Clear();

            if(FlatRequiredDate.SelectedDate== null)
            {
                MessageBox.Show("Please select a due date.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (QuantityRequired < 1)
            {
                MessageBox.Show("Please enter a quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!multi)
            {
                newDrops.Add(new Drop()
                {
                    Quantity = QuantityRequired,
                    DateRequired = (DateTime)FlatRequiredDate.SelectedDate
                });
            }


            dropsListBox.ItemsSource = newDrops;
        }

        private void CallOffDropQuantity_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
            if(Int32.TryParse(TotalQuantityTextBox.Text, out int j))
            {
                QuantityRequired = j;
            }
        }

        private void TotalQuantityTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void refreshDropsButton_Click(object sender, RoutedEventArgs e)
        {
            bool isMulti = (bool)multiRadio.IsChecked;
            bool doByDayNumber = (bool)byDayNumber.IsChecked;

            CalculateDrops(isMulti, doByDayNumber);
        }
    }
}
