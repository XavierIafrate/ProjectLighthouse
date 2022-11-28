using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Windows;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class EditProductWindow : Window
    {
        public TurnedProduct Product { get; set; }
        public List<BarStock> BarStock { get; set; }
        public bool SaveExit = false;

        public EditProductWindow(TurnedProduct product)
        {
            InitializeComponent();

            Product = product;
            BarStock = DatabaseHelper.Read<BarStock>();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Product.ValidateAll();

            if (Product.HasErrors)
            {
                return;
            }

            SaveExit = true;
            Close();
        }
    }
}
