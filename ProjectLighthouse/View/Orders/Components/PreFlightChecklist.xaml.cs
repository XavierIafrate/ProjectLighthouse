using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class PreFlightChecklist : UserControl, INotifyPropertyChanged
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(PreFlightChecklist), new PropertyMetadata(null, SetValues));



        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(PreFlightChecklist), new PropertyMetadata(false, SetEditMode));


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<BarStock> baseBarStock;
        private List<BarStock> barStock;
        public List<BarStock> BarStock
        {
            get { return barStock; }
            set { barStock = value; OnPropertyChanged(); }
        }

        private List<string> requirementTrace = new();
        public List<string> RequirementTrace
        {
            get { return requirementTrace; }
            set { requirementTrace = value; OnPropertyChanged(); }
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PreFlightChecklist control) return;
            if (control.Order == null)
            {
                control.BarStock = new();
                return;
            }

            control.BarStock = control.baseBarStock.Where(x => x.MaterialId == control.Order.MaterialId && x.Size >= control.Order.MajorDiameter && x.IsHexagon == (control.Order.ProductGroup ?? new()).UsesHexagonBar).ToList();
            control.barComboBox.SelectedValue = control.BarStock.Find(x => x.Id == control.Order.BarID);
            control.BarSelection.IsEnabled = control.Order.NumberOfBarsIssued == 0;

            control.GetRequirementTrace();

            control.SetEnabled();
        }

        private void GetRequirementTrace()
        {
            List<string> result = new();

            if (Order.Product.RequiresFeaturesList.Count > 0)
            {
                foreach (string feature in Order.Product.RequiresFeaturesList)
                {
                    result.Add($"Product '{Order.Product.Name}' requires machine feature '{feature}'");
                }
            }

            if (Order.ProductGroup.RequiresFeaturesList.Count > 0)
            {
                foreach (string feature in Order.ProductGroup.RequiresFeaturesList)
                {
                    result.Add($"Product Group '{Order.ProductGroup.Name}' requires machine feature '{feature}'");
                }
            }

            if (Order.Bar.RequiresFeaturesList.Count > 0)
            {
                foreach (string feature in Order.Bar.RequiresFeaturesList)
                {
                    result.Add($"Bar Stock '{Order.Bar.Id}' requires machine feature '{feature}'");
                }
            }

            if (Order.Bar.MaterialData.RequiresFeaturesList.Count > 0)
            {
                foreach (string feature in Order.Bar.MaterialData.RequiresFeaturesList)
                {
                    result.Add($"Material '{Order.Bar.MaterialData.MaterialCode}' requires machine feature '{feature}'");
                }
            }

            RequirementTrace = result;
        }

        private static void SetEditMode(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PreFlightChecklist control) return;
            control.SetEnabled();
        }

        private void SetEnabled()
        {
            if (EditMode)
            {
                if (Order.AssignedTo == null)
                {
                    EditLockBadge.Visibility = Visibility.Collapsed;
                    this.IsEnabled = true;
                }
                else
                {
                    bool canEdit = (App.CurrentUser.HasPermission(Model.Core.PermissionType.EditOrder)
                        || Order.AssignedTo == App.CurrentUser.UserName) && Order.State < OrderState.Running;

                    EditLockBadge.Visibility = !canEdit
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    this.IsEnabled = canEdit;
                }
            }
            else
            {
                EditLockBadge.Visibility = Visibility.Collapsed;
                this.IsEnabled = false;
            }
        }

        public PreFlightChecklist()
        {
            InitializeComponent();

            baseBarStock = DatabaseHelper.Read<BarStock>().OrderBy(x => x.Id).ToList();
            List<MaterialInfo> materials = DatabaseHelper.Read<MaterialInfo>();

            baseBarStock.ForEach(x => x.MaterialData = materials.Find(m => m.Id == x.MaterialId));
        }

        private void barComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BarWarningText.Visibility = Visibility.Collapsed;

            if (sender is not ComboBox comboBox) return;
            if (comboBox.SelectedValue is not string barId) return;

            BarStock? bar = BarStock.Find(x => x.Id == barId);

            if (bar == null) return;

            if (bar.Size > (Order.MajorDiameter + 5))
            {
                BarWarningText.Visibility = Visibility.Visible;
            }
        }
    }
}
