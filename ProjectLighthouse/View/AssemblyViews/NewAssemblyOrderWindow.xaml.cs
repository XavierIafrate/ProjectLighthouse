using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<BillOfMaterialsItem> BOMItems { get; set; }

        public NewAssemblyOrderWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadData();

            newDrops = new List<Drop>();
            QuantityRequired = 0;
        }

        private void LoadData()
        {
            Products = DatabaseHelper.Read<AssemblyItem>().ToList();
            BOMItems = DatabaseHelper.Read<BillOfMaterialsItem>().ToList();

            TreeItems = new List<AssemblyGroup>();
            List<string> groups = new List<string>();
            foreach (AssemblyItem product in Products)
            {
                if (!groups.Contains(product.ProductGroup))
                    groups.Add(product.ProductGroup);
            }

            foreach (string group in groups) // probably not necessary
            {
                TreeItems.Add(new AssemblyGroup()
                {
                    group = group,
                    items = new List<AssemblyItem>(Products.Where(n => n.ProductGroup == group))
                });
            }

            foreach (AssemblyGroup group in TreeItems)
            {
                TreeViewItem ParentItem = new TreeViewItem();
                ParentItem.Header = group.group;
                ParentItem.Focusable = false;
                foreach (AssemblyItem item in group.items)
                    ParentItem.Items.Add(new TreeViewItem { Header = item.ProductNumber });
                AssemblyTree.Items.Add(ParentItem);
            }
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            if (multiRadio == null) // not loaded yet
                return;

            bool isMulti = (bool)multiRadio.IsChecked;
            if (isMulti)
            {
                interval_radio.IsEnabled = true;
                cardinal_radio.IsEnabled = true;
                interval_radio.IsChecked = true;
                cardinal_radio.IsChecked = false;
                CallOffDropQuantity.IsEnabled = true;
                MultiDropStartingDate.IsEnabled = true;
                interval_textbox.IsEnabled = true;
                FlatRequiredDate.IsEnabled = false;
            }
            else
            {
                interval_radio.IsEnabled = false;
                cardinal_radio.IsEnabled = false;
                Radio_Checked_CallOff(new object(), new RoutedEventArgs());
                interval_radio.IsChecked = false;
                cardinal_radio.IsChecked = false;
                CallOffDropQuantity.IsEnabled = false;
                MultiDropStartingDate.IsEnabled = false;
                interval_textbox.IsEnabled = false;
                FlatRequiredDate.IsEnabled = true;
            }
        }

        private void Radio_Checked_CallOff(object sender, RoutedEventArgs e)
        {
            bool isCardinal = (bool)cardinal_radio.IsChecked;
            if (!isCardinal)
            {
                cardinal_combobox.Text = "";
                DayOfWeekComboBox.Text = "";
            }
            cardinal_combobox.IsEnabled = isCardinal;
            DayOfWeekComboBox.IsEnabled = isCardinal;
            interval_textbox.IsEnabled = !isCardinal;
        }

        private void CalculateDrops(bool multi, bool byInterval = false)
        {
            newDrops.Clear();
            //dropsItemsControl.ItemsSource = newDrops;

            if (QuantityRequired < 1)
            {
                MessageBox.Show("Please enter a quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!multi)
            {

                if (FlatRequiredDate.SelectedDate == null)
                {
                    MessageBox.Show("Please select a due date.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                newDrops.Add(new Drop()
                {
                    Quantity = QuantityRequired,
                    DateRequired = (DateTime)FlatRequiredDate.SelectedDate
                });
            }
            else
            {
                // arrange quantity drops
                if (!Int32.TryParse(CallOffDropQuantity.Text, out int drop_quantity))
                {
                    MessageBox.Show("Please enter a drop quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                int num_solid_drops = (int)Math.Floor((double)QuantityRequired / (double)drop_quantity);
                int trailing_quantity = QuantityRequired % drop_quantity;

                //Debug.WriteLine($"Number of Drops: {num_solid_drops}");
                //Debug.WriteLine($"Trailing Quantity: {trailing_quantity}");

                for (int i = 0; i < num_solid_drops; i++)
                {
                    newDrops.Add(new Drop() { Quantity = drop_quantity });
                }

                if (trailing_quantity != 0)
                    newDrops.Add(new Drop() { Quantity = trailing_quantity });


                // arrange dates

                if (!MultiDropStartingDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please select a starting date for the drops.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (byInterval)
                {
                    if (!Int32.TryParse(interval_textbox.Text, out int interval))
                    {
                        MessageBox.Show("Please enter an interval for the drops.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    int j = 0;
                    foreach (Drop drop in newDrops)
                    {
                        drop.DateRequired = MultiDropStartingDate.SelectedDate.Value.AddDays(7 * j * interval);
                        j++;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(cardinal_combobox.Text))
                    {
                        MessageBox.Show("Please select a cardinal for the drop frequency.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    if (string.IsNullOrEmpty(DayOfWeekComboBox.Text))
                    {
                        MessageBox.Show("Please select a day of th week for the drop.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    ComboBoxItem selected_day_week = (ComboBoxItem)DayOfWeekComboBox.SelectedValue;
                    Int32.TryParse(selected_day_week.Tag.ToString(), out int day_of_week);

                    ComboBoxItem selected_cardinal = (ComboBoxItem)cardinal_combobox.SelectedValue;
                    Int32.TryParse(selected_cardinal.Tag.ToString(), out int week_num);

                    DateTime starting_date = MultiDropStartingDate.SelectedDate.Value;

                    int i = 0;

                    foreach (Drop d in newDrops)
                    {
                        DateTime first_day_of_seed_month = new DateTime(starting_date.Year, starting_date.Month, 1).AddMonths(i);
                        DateTime drop_due_date = GetNthDayOfMonth(week_num, (DayOfWeek)day_of_week, first_day_of_seed_month);
                        if (drop_due_date < starting_date)
                        {
                            i++;
                            first_day_of_seed_month = new DateTime(starting_date.Year, starting_date.Month, 1).AddMonths(i);
                            drop_due_date = GetNthDayOfMonth(week_num, (DayOfWeek)day_of_week, first_day_of_seed_month);
                        }

                        d.DateRequired = drop_due_date;
                        i++;
                    }
                }
            }

            dropsItemsControl.ItemsSource = null;
            dropsItemsControl.ItemsSource = newDrops;
            EnableNewOrder();
        }

        public void EnableNewOrder()
        {
            createOrderButton.IsEnabled = (newDrops.Count > 0 && AssemblyTree.SelectedItem != null);
        }

        public DateTime GetNthDayOfMonth(int occurrance, DayOfWeek dayOfWeek, DateTime firstDayOfMonth)
        {
            DateTime i = firstDayOfMonth;
            DateTime result = new();
            int found = 0;

            while (i.Month == firstDayOfMonth.Month)
            {
                if (i.DayOfWeek == dayOfWeek)
                {
                    result = i;
                    found += 1;
                    if (found == occurrance)
                    {
                        break;
                    }
                }

                i = i.AddDays(1);
            }

            return result;
        }

        public int GetDayOfWeekAsInt(DateTime dateTime)
        {
            return (int)(dateTime.DayOfWeek + 6) % 7; // shift so Mon=0, Sun=7
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
            bool byInterval = (bool)interval_radio.IsChecked;

            CalculateDrops(isMulti, byInterval);
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

        private void AssemblyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // don't allow parents (difficult)
            EnableNewOrder();
        }

        private void createOrderButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedProduct_obj = (TreeViewItem)AssemblyTree.SelectedValue;
            string selectedProduct = selectedProduct_obj.Header.ToString();
            AssemblyItem targetProduct = Products.SingleOrDefault(n => n.ProductNumber == selectedProduct);

            MessageBoxResult Result = MessageBox.Show($"Are you sure you want to create an order for {QuantityRequired}pcs of {selectedProduct} across {newDrops.Count} drops?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
            {
                AssemblyManufactureOrder newOrder = new() { Name = GetNewAssemblyOrderName(), CreatedAt = DateTime.Now, CreatedBy = App.CurrentUser.GetFullName(), Status = "Problem", ModifiedAt = DateTime.Now, RequiredProduct = selectedProduct };
                DatabaseHelper.Insert<AssemblyManufactureOrder>(newOrder);

                List<AssemblyItemExpansion> newOrderItems = new();

                newOrderItems.Add(new AssemblyItemExpansion() { Item = targetProduct, Checked = false, Quantity = QuantityRequired });


                //expansion
                bool complete = false;
                while (!complete)
                {
                    List<AssemblyItemExpansion> tmpItems = new();
                    foreach (AssemblyItemExpansion x in newOrderItems)
                    {
                        complete = true;
                        if (!x.Checked)
                        {
                            complete = false;
                            tmpItems.AddRange(GetChildren(x.Item, x.Quantity));
                            x.Checked = true;
                        };
                    }

                    newOrderItems.AddRange(tmpItems);
                }

                foreach (AssemblyItemExpansion x in newOrderItems)
                {
                    DatabaseHelper.Insert<AssemblyOrderItem>(new()
                    {
                        OrderReference = newOrder.Name,
                        ProductName = x.Item.ProductNumber,
                        QuantityRequired = x.Quantity,
                        QuantityReady = 0,
                        ChildOf = x.Parent
                    });
                }

                foreach (Drop d in newDrops)
                {
                    d.ForOrder = newOrder.Name;
                    d.Product = selectedProduct;
                    DatabaseHelper.Insert<Drop>(d);
                }

                this.Close();
                MessageBox.Show($"Successfully created Assembly Order {newOrder.Name}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private List<AssemblyItemExpansion> GetChildren(AssemblyItem parent, int parent_quantity)
        {
            List<BillOfMaterialsItem> items = BOMItems.Where(n => n.BOMID == parent.BillOfMaterials).ToList();
            List<AssemblyItemExpansion> result = new();
            if (items.Count > 0)
            {
                foreach (BillOfMaterialsItem i in items)
                {
                    result.Add(new() { Checked = false, Item = Products.SingleOrDefault(n => n.ProductNumber == i.ComponentItem), Quantity = i.Quantity * parent_quantity, Parent = parent.ProductNumber });
                    if (result.Last().Item == null)
                        Debug.WriteLine($"{i.ComponentItem} not found");
                }
            }

            return result;
        }

        private static string GetNewAssemblyOrderName()
        {
            int orderNumber = DatabaseHelper.Read<AssemblyManufactureOrder>().ToList().Count + 1;
            string strOrderNumber = orderNumber.ToString();
            const string blank = "AM00000";
            return blank.Substring(0, 7 - strOrderNumber.Length) + strOrderNumber;
        }

        private class AssemblyItemExpansion
        {
            public AssemblyItem Item { get; set; }
            public string Parent { get; set; }
            public int Quantity { get; set; }
            public bool Checked { get; set; }
        }

    }
}
