using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Windows;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class EditProductWindow : Window
    {
        public TurnedProduct Product { get; set; }
        public List<BarStock> BarStock { get; set; }
        public EditProductWindow(TurnedProduct product)
        {
            InitializeComponent();

            Product = product;
            BarStock = DatabaseHelper.Read<BarStock>();
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Product.DataIsComplete())
            {
                MessageBox.Show("Please fill out all of the fields before saving.", "More data required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DatabaseHelper.Update(Product);
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (double.TryParse(MajorDiameterTextBox.Text, out double diameter))
            {
                Product.MajorDiameter = diameter;
            }

            if (double.TryParse(MajorLengthTextBox.Text, out double length))
            {
                Product.MajorLength = length;
            }
        }
    }
}
