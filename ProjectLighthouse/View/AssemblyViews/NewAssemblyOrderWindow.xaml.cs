using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public List<AssemblyGroup> TreeItems { get; set; }

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
            TreeItems = new List<AssemblyGroup>();
            List<string> groups = new List<string>();
            foreach (AssemblyItem product in Products)
            {
                if (!groups.Contains(product.ProductGroup))
                    groups.Add(product.ProductGroup);
            }

            foreach (string group in groups)
            {
                TreeItems.Add(new AssemblyGroup()
                {
                    group = group,
                    items = new List<AssemblyItem>(Products.Where(n => n.ProductGroup == group))
                });
            }

            AssemblyTree.ItemsSource = TreeItems;
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
            //dropsItemsControl.ItemsSource = newDrops;

            if (FlatRequiredDate.SelectedDate == null)
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


            dropsItemsControl.ItemsSource = newDrops;
        }

        private void CallOffDropQuantity_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
            if (Int32.TryParse(TotalQuantityTextBox.Text, out int j))
            {
                QuantityRequired = j;
            }
        }

        private void Validate_NumsOnly(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void refreshDropsButton_Click(object sender, RoutedEventArgs e)
        {
            bool isMulti = (bool)multiRadio.IsChecked;
            bool doByDayNumber = (bool)byDayNumber.IsChecked;

            CalculateDrops(isMulti, doByDayNumber);
        }

        public class AssemblyGroup
        {
            public string group { get; set; }
            public List<AssemblyItem> items { get; set; }
        }

        private void TotalQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(TotalQuantityTextBox.Text, out int j))
            {
                QuantityRequired = j;
            }
        }
    }
}
