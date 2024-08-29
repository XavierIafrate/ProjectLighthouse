using DocumentFormat.OpenXml.Wordprocessing;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Drawings;
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
            if (e.OldValue is LatheManufactureOrder oldOrder)
            {
                oldOrder.PropertyChanged -= control.OrderPropertyChanged;
                foreach (LatheManufactureOrderItem item in oldOrder.OrderItems)
                {
                    item.PropertyChanged -= control.OrderItemChanged;
                }
                foreach (Lot lot in oldOrder.Lots)
                {
                    lot.PropertyChanged -= control.OrderLotChanged;
                }
            }

            if (control.Order is null) return;
            control.OrderTabControl.SelectedIndex = control.Order.State < OrderState.Running ? 0 : 3;
            if (e.NewValue is LatheManufactureOrder newOrder)
            {
                newOrder.PropertyChanged += control.OrderPropertyChanged;
                foreach (LatheManufactureOrderItem item in newOrder.OrderItems)
                {
                    item.PropertyChanged += control.OrderItemChanged;
                }
                foreach (Lot lot in newOrder.Lots)
                {
                    lot.PropertyChanged += control.OrderLotChanged;
                }
            }

        }

        private void OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] watchForTimeOrBarRecalc = new string[]
            {
                nameof(LatheManufactureOrder.Breakdowns),
                nameof(LatheManufactureOrder.TimeCodePlanned),
                nameof(LatheManufactureOrder.SpareBars),
                nameof(LatheManufactureOrder.Lots),
                nameof(LatheManufactureOrder.OrderItems),
            };

            if (watchForTimeOrBarRecalc.Contains(e.PropertyName))
            {
                CalculateTimeAndBar();
            }

            if (e.PropertyName == nameof(LatheManufactureOrder.Lots))
            {
                foreach (Lot lot in Order.Lots)
                {
                    lot.PropertyChanged -= OrderLotChanged;
                    lot.PropertyChanged += OrderLotChanged;
                }

                foreach (LatheManufactureOrderItem item in Order.OrderItems)
                {
                    List<Lot> itemsLots = Order.Lots.Where(x => x.ProductName == item.ProductName).ToList();
                    item.QuantityDelivered = itemsLots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
                    item.QuantityMade = itemsLots.Where(x => !x.IsReject).Sum(x => x.Quantity);
                    item.QuantityReject = itemsLots.Where(x => x.IsReject).Sum(x => x.Quantity);

                    item.PropertyChanged -= OrderItemChanged;
                    item.PropertyChanged += OrderItemChanged;
                }
            }
            if (e.PropertyName == nameof(LatheManufactureOrder.OrderItems))
            {
                foreach (LatheManufactureOrderItem item in Order.OrderItems)
                {
                    item.PropertyChanged -= OrderItemChanged;
                    item.PropertyChanged += OrderItemChanged;
                }
            }
        }

        private void OrderItemChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] watchForTimeOrBarRecalc = new string[]
            {
                nameof(LatheManufactureOrderItem.CycleTime),
                nameof(LatheManufactureOrderItem.PreviousCycleTime),
                nameof(LatheManufactureOrderItem.ModelledCycleTime),
                nameof(LatheManufactureOrderItem.MajorLength),
                nameof(LatheManufactureOrderItem.PartOffLength),
                nameof(LatheManufactureOrderItem.RequiredQuantity),
                nameof(LatheManufactureOrderItem.TargetQuantity),
            };

            if (watchForTimeOrBarRecalc.Contains(e.PropertyName))
            {
                CalculateTimeAndBar();
            }
        }

        private void OrderLotChanged(object sender, PropertyChangedEventArgs e)
        {
            string[] watchForTimeOrBarRecalc = new string[]
            {
                nameof(Lot.CycleTime),
                nameof(Lot.Quantity),
                nameof(Lot.IsAccepted),
                nameof(Lot.IsReject),
            };

            if (watchForTimeOrBarRecalc.Contains(e.PropertyName))
            {
                CalculateTimeAndBar();

                foreach (LatheManufactureOrderItem item in Order.OrderItems)
                {
                    List<Lot> itemsLots = Order.Lots.Where(x => x.ProductName == item.ProductName).ToList();
                    item.QuantityDelivered = itemsLots.Where(x => x.IsDelivered).Sum(x => x.Quantity);
                    item.QuantityMade = itemsLots.Where(x => !x.IsReject).Sum(x => x.Quantity);
                    item.QuantityReject = itemsLots.Where(x => x.IsReject).Sum(x => x.Quantity);
                }
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
            AddItemToOrderWindow window = new(Order) { Owner = App.MainViewModel.MainWindow };

            if (window.PossibleItems.Count == 0)
            {
                MessageBox.Show("No further items are available to run on this order.", "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            window.ShowDialog();

            CalculateTimeAndBar();
        }

        private void CalculateTimeAndBar()
        {
            double? partOff = null;
            if (Order.AssignedLathe is not null)
            {
                partOff = Order.AssignedLathe.PartOff;
            }
            
            if (partOff is not null)
            {
                Order.NumberOfBars = Order.OrderItems.CalculateNumberOfBars(Order.Bar, Order.SpareBars, (double)partOff);
            }
            else
            {
                Order.NumberOfBars = Order.OrderItems.CalculateNumberOfBars(Order.Bar, Order.SpareBars);
            }

            Order.TimeToComplete = OrderResourceHelper.CalculateOrderRuntime(Order, Order.OrderItems, Order.Breakdowns, Order.Lots);
        }


    }
}
