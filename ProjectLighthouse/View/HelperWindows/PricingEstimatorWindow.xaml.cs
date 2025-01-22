using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class PricingEstimatorWindow : Window, INotifyPropertyChanged
    {
        private TurnedProduct item;
        public TurnedProduct Item
        {
            get { return item; }
            set { item = value; OnPropertyChanged(); }
        }

        private List<MaterialInfo> materials;
        public List<MaterialInfo> Materials
        {
            get { return materials; }
            set { materials = value; OnPropertyChanged(); }
        }

        private MaterialInfo selectedMaterial;

        public MaterialInfo SelectedMaterial
        {
            get { return selectedMaterial; }
            set { selectedMaterial = value; Item.MaterialId = value.Id; OnPropertyChanged(); }
        }

        private List<BarStock> barStock;

        private bool cannotCalculate;
        public bool CannotCalculate
        {
            get { return cannotCalculate; }
            set { cannotCalculate = value; OnPropertyChanged(); }
        }

        private bool noBarAvailable;
        public bool NoBarAvailable
        {
            get { return noBarAvailable; }
            set { noBarAvailable = value; OnPropertyChanged();}
        }

        private bool noMaterialCost;
        public bool NoMaterialCost
        {
            get { return noMaterialCost; }
            set { noMaterialCost = value; OnPropertyChanged(); }
        }

        private bool useHexBar;

        public bool UseHexBar
        {
            get { return useHexBar; }
            set { useHexBar = value; CostItem(); SetUseHexUI(); OnPropertyChanged(); }
        }

        private void SetUseHexUI()
        {
            MajorDiameterLabel.Text = UseHexBar ? "Across Flats Size" : "Major Diameter";
            MajorDiameterTextBox.Tag = UseHexBar ? "across flats size" : "major diameter";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PricingEstimatorWindow()
        {
            InitializeComponent();
            Item = new() 
            { 
                MajorDiameter = 10,
                MajorLength = 20,
                MaterialId = 1,
                CycleTime = 90,
            };

            Materials = DatabaseHelper.Read<MaterialInfo>();
            barStock = DatabaseHelper.Read<BarStock>();
            barStock.ForEach(x => x.MaterialData = Materials.Find(m => m.Id == x.MaterialId));
            RemainderText.Text = $"{App.Constants.BarRemainder:#,##0}";
            SelectedMaterial = Materials.Find(x => x.Id == Item.MaterialId);

            CycleTimeControl.PropertyChanged += CycleTimeControl_PropertyChanged;
        }

        private void CycleTimeControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CostItem();
        }

        void CostItem()
        {
            if (SelectedMaterial.Cost is null)
            {
                CannotCalculate = true;
                NoMaterialCost = true;
                return;
            }

            NoMaterialCost = false;

            BarStock? bar = GetRequiredBarStock(barStock, SelectedMaterial.Id, Item.MajorDiameter, hexBar: UseHexBar);
            if (bar is null)
            {
                CannotCalculate = true;
                NoBarAvailable = true;
                return;
            }

            NoBarAvailable = false;

            TimeModel model = new() { Floor = Item.CycleTime };
            
            double materialBudget = item.MajorLength + item.PartOffLength + 2;
            Item.ItemCost = new TurnedProduct.Cost(SelectedMaterial, bar, model, Item.MajorLength, materialBudget);

            CannotCalculate = false;
        }

        static BarStock? GetRequiredBarStock(List<BarStock> bars, int materialId, double size, bool hexBar)
        {
            if (hexBar)
            {
                return bars.Find(x => x.IsHexagon && x.Size == size && x.MaterialId == materialId);
            }


            bars = bars.Where(x => x.MaterialId == materialId && x.MajorDiameter >= size && !x.IsHexagon)
                        .OrderBy(x => x.Size)
                        .ToList();

            if (bars.Count == 0)
            {
                return null;
            }

            return bars.First();
        }

        private void InputChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CostItem();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CostItem();
        }
    }
}
