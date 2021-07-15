using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for NewAssemblyProductWindow.xaml
    /// </summary>
    public partial class NewAssemblyProductWindow : Window
    {
        public List<AssemblyItem> items { get; set; }
        public AssemblyItem tmpItem { get; set; }
        public bool addedNew = false;

        public NewAssemblyProductWindow()
        {
            InitializeComponent();
            items = new List<AssemblyItem>();
            tmpItem = new AssemblyItem();
            LoadContent();
            EnableAddButton();
        }

        public void LoadContent()
        {
            List<string> groups = new List<string>();
            items = DatabaseHelper.Read<AssemblyItem>().ToList();
            foreach (AssemblyItem item in items)
            {
                if (!groups.Contains(item.ProductGroup))
                    groups.Add(item.ProductGroup);
            }
            ProductGroupComboBox.ItemsSource = groups;
        }

        private void ProductGroupTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ProductGroupComboBox.SelectedValue = "";
            ProductGroupGhost.Visibility = string.IsNullOrEmpty(ProductGroupTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
            int selection = ProductGroupTextBox.SelectionStart;
            ProductGroupTextBox.Text = ProductGroupTextBox.Text.ToUpper();
            ProductGroupTextBox.SelectionStart = selection;
            EnableAddButton();
        }

        public void EnableAddButton()
        {
            AssignValues();
            AddButton.IsEnabled = !string.IsNullOrWhiteSpace(tmpItem.ProductNumber) && !string.IsNullOrWhiteSpace(tmpItem.ProductGroup);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AssignValues();
            AssignRoutingsAndBillOfMaterials();
            DatabaseHelper.Insert<AssemblyItem>(tmpItem);
            addedNew = true;
            Close();
        }

        private void AssignRoutingsAndBillOfMaterials()
        {
            int numProducts = DatabaseHelper.Read<AssemblyItem>().Count;
            numProducts += 1;
            tmpItem.BillOfMaterials = string.Format("{0}{1}", "BOM00000".Substring(0, 8 - numProducts.ToString().Length), numProducts);
            tmpItem.Routing = string.Format("{0}{1}", "R00000".Substring(0, 6 - numProducts.ToString().Length), numProducts);
        }

        private void AssignValues()
        {
            tmpItem.ProductNumber = ProductNumberTextBox.Text;
            if (ProductGroupComboBox.SelectedValue == null)
            {
                tmpItem.ProductGroup = ProductGroupTextBox.Text;
            }
            else if (!string.IsNullOrWhiteSpace(ProductGroupTextBox.Text))
            {
                tmpItem.ProductGroup = ProductGroupTextBox.Text;
            }
            else
            {
                tmpItem.ProductGroup = ProductGroupComboBox.SelectedValue.ToString();
            }
            tmpItem.Description = ProductDescriptionTextBox.Text;
        }

        private void ProductNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            productNumberGhost.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Hidden;
            EnableAddButton();
        }

        private void ProductGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableAddButton();
        }
    }
}
