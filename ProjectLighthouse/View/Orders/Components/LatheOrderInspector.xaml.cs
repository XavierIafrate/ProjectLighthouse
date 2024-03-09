using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class LatheOrderInspector : UserControl, INotifyPropertyChanged
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(LatheOrderInspector), new PropertyMetadata(null, SetOrder));

        private static void SetOrder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LatheOrderInspector control) return;
            if (control.Order is null) return;
            if (control.Order.State < OrderState.Running)
            {
                control.OrderTabControl.SelectedIndex = 0;
            }
            else
            {
                control.OrderTabControl.SelectedIndex = 3;
            }
        }

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(LatheOrderInspector), new PropertyMetadata(false, SetEditMode));

        private static void SetEditMode(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LatheOrderInspector control) return;
            if (control.EditMode is false)
            {
                control.SelectedItem = null;
            }
        }


        private LatheManufactureOrderItem? selectedItem;
        public LatheManufactureOrderItem? SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged(); }
        }


        public List<User> ProductionStaff
        {
            get { return (List<User>)GetValue(ProductionStaffProperty); }
            set { SetValue(ProductionStaffProperty, value); }
        }

        public static readonly DependencyProperty ProductionStaffProperty =
            DependencyProperty.Register("ProductionStaff", typeof(List<User>), typeof(LatheOrderInspector), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool HasConfigPermission { get; set; } = App.CurrentUser.HasPermission(Model.Core.PermissionType.EditOrder);

        public LatheOrderInspector()
        {
            InitializeComponent();
        }

        private void UpdateDrawingsButton_Click(object sender, RoutedEventArgs e)
        {
            List<TechnicalDrawing> allDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.DrawingType == (Order.IsResearch ? TechnicalDrawing.Type.Research : TechnicalDrawing.Type.Production)).ToList();
            List<TechnicalDrawing> drawings = TechnicalDrawing.FindDrawings(allDrawings, Order.OrderItems, Order.GroupId, Order.MaterialId);

            int[] currentDrawingIds = Order.DrawingsReferences.Select(x => x.DrawingId).OrderBy(x => x).ToArray();
            int[] upToDateDrawingIds = drawings.Select(x => x.Id).OrderBy(x => x).ToArray();

            if (currentDrawingIds.Length == upToDateDrawingIds.Length)
            {
                bool diff = false;
                for (int i = 0; i < currentDrawingIds.Length; i++)
                {
                    if (currentDrawingIds[i] != upToDateDrawingIds[i])
                    {
                        diff = true;
                        break;
                    }
                }

                if (!diff)
                {
                    MessageBox.Show("No drawing updates have been found.", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else if (currentDrawingIds.Length == 0 && upToDateDrawingIds.Length == 0)
            {
                MessageBox.Show("No drawing updates have been found.", "Up to date", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            for (int i = 0; i < currentDrawingIds.Length; i++)
            {
                if (!upToDateDrawingIds.Contains(currentDrawingIds[i]))
                {
                    Order.Drawings.Remove(Order.Drawings.Find(x => x.Id == currentDrawingIds[i]));
                    Order.DrawingsReferences.Remove(Order.DrawingsReferences.Find(x => x.DrawingId == currentDrawingIds[i]));
                }
            }


            for (int i = 0; i < upToDateDrawingIds.Length; i++)
            {
                if (!currentDrawingIds.Contains(upToDateDrawingIds[i]))
                {
                    OrderDrawing newRecord = new() { DrawingId = upToDateDrawingIds[i], OrderId = Order.Name };
                    Order.DrawingsReferences.Add(newRecord);
                    Order.Drawings.Add(drawings.Find(x => x.Id == upToDateDrawingIds[i]));
                }
            }

            Order.Drawings = Order.Drawings.ToList();

            MessageBox.Show("Updated drawings were found and the order records amended.", "Now up to date", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is null) return;
            Order.OrderItems.Remove(SelectedItem);
            Order.OrderItems = Order.OrderItems.ToList();
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemToOrderWindow window = new(Order.Id);
            if (window.PossibleItems.Count == 0)
            {
                MessageBox.Show("No further items are available to run on this order.", "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            window.ShowDialog();

            //if (window.ItemsWereAdded)
            //{
            //    SaveExit = true;
            //    CalculateTimeAndBar();
            //    SaveOrder();
            //    LoadData(Order.Name);
            //}
        }

        private void CalculateTimeAndBar()
        {
            Order.TimeToComplete = OrderResourceHelper.CalculateOrderRuntime(Order, Items, Breakdowns);

            double? partOff = null;
            if (!string.IsNullOrEmpty(Order.AllocatedMachine))
            {
                Lathe? runningOnLathe = Lathes.Find(l => l.Id == Order.AllocatedMachine);
                if (runningOnLathe is not null)
                {
                    partOff = runningOnLathe.PartOff;
                }
            }

            if (partOff is not null)
            {
                Order.NumberOfBars = Items.CalculateNumberOfBars((BarStock)BarStockComboBox.SelectedValue, Order.SpareBars, (double)partOff);
            }
            else
            {
                Order.NumberOfBars = Items.CalculateNumberOfBars((BarStock)BarStockComboBox.SelectedValue, Order.SpareBars);
            }
        }


    }
}
