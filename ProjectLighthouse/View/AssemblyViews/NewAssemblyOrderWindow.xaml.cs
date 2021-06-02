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
        public List<AssemblyGroup> TreeItems { get; set; }

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
            TreeItems = new List<AssemblyGroup>();
            List<string> groups = new List<string>();
            foreach(AssemblyItem product in Products)
            {
                if (!groups.Contains(product.ProductGroup))
                    groups.Add(product.ProductGroup);
            }

            foreach(string group in groups)
            {
                TreeItems.Add(new AssemblyGroup()
                {
                    group = group,
                    items = new List<AssemblyItem>(Products.Where(n=> n.ProductGroup == group))
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
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        public class AssemblyGroup
        { 
            public string group { get; set; }
            public List<AssemblyItem> items { get; set; }
        }

    }
}
