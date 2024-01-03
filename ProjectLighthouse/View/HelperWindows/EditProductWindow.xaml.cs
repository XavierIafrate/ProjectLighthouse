using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class EditProductWindow : Window, INotifyPropertyChanged
    {
        public TurnedProduct Product { get; set; }
        public List<MaterialInfo> MaterialInfo { get; set; }

        public List<ProductGroup> ProductGroups { get; set; }

        private ProductGroup selectedGroup;

        public ProductGroup SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                Product.GroupId = SelectedGroup?.Id;
                OnPropertyChanged();
            }
        }


        public bool SaveExit = false;



        public EditProductWindow(TurnedProduct product)
        {
            Product = product;

            MaterialInfo = DatabaseHelper.Read<MaterialInfo>();
            ProductGroups = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .ToList();

            SelectedGroup = ProductGroups.Find(x => x.Id == product.GroupId);

            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Product.ValidateForOrder();
            SaveExit = true;
            Close();
        }

        private void Material_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Product.ValidateProperty("MaterialId");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
