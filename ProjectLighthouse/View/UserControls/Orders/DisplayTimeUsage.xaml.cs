using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayTimeUsage : UserControl
    {
        public List<LatheManufactureOrderItem> OrderItems
        {
            get { return (List<LatheManufactureOrderItem>)GetValue(OrderItemsProperty); }
            set { SetValue(OrderItemsProperty, value); }
        }

        public static readonly DependencyProperty OrderItemsProperty =
            DependencyProperty.Register("OrderItems", typeof(List<LatheManufactureOrderItem>), typeof(DisplayTimeUsage), new PropertyMetadata(null, SetValues));


        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(DisplayTimeUsage), new PropertyMetadata(null, SetValues));


        public DisplayTimeUsage()
        {
            InitializeComponent();
        }


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayTimeUsage control) return;

            control.Draw();
        }

        private void Draw()
        {
            this.MainGrid.Children.Clear();
            this.MainGrid.ColumnDefinitions.Clear();
            this.MainGrid.RowDefinitions.Clear();

            if (Order is null) return;
            if (OrderItems is null) return;
            if (OrderItems.Count == 0) return;

            if (OrderItems.First().AssignedMO != Order.Name) return;
            if (Order.State != OrderState.Complete) return;

            Dictionary<LatheManufactureOrderItem, TimeSpan> spans = new();

            TimeSpan settingSpan = TimeSpan.FromSeconds(6 * 3600);
            TimeSpan orderSpan = Order.EndsAt() - Order.StartDate;

            for (int i = 0; i < OrderItems.Count; i++)
            {
                LatheManufactureOrderItem item = OrderItems[i];

                spans.Add(item, TimeSpan.FromSeconds((item.QuantityMade + item.QuantityReject) * item.CycleTime));
            }

            Dictionary<string, int> spanMap = DrawGrid(settingSpan, orderSpan, spans);
            DrawEvents(orderSpan, spans, spanMap);
        }

        private Dictionary<string, int> DrawGrid(TimeSpan setting, TimeSpan order, Dictionary<LatheManufactureOrderItem, TimeSpan> items)
        {

            // Rows
            MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

            TimeSpan totalItems = TimeSpan.Zero;
            for (int i = 0; i < items.Count; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

                TextBlock rowHeader = new()
                {
                    Text = items.ElementAt(i).Key.ProductName,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new(5, 0, 5, 0),
                };

                totalItems += items.ElementAt(i).Value;

                AddToMainGrid(rowHeader, 0, i + 1);
            }

            MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

            bool overdelivered = totalItems > order;
            TimeSpan prevailing = overdelivered ? totalItems : order;

            double unitsPerHour = 100 / (setting + prevailing).TotalHours;

            double remainingHours = Math.Abs((order - totalItems).TotalHours);

            // Columns
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * setting.TotalHours, GridUnitType.Star) });

            TimeSpan itemsTotal = TimeSpan.Zero;
            Dictionary<string, int> spans = new();
            bool latch = false;
            int addedColumns = 0;
            int diffStarts = 0;

            for (int i = 0; i < items.Count; i++)
            {
                itemsTotal += items.ElementAt(i).Value;
                if (itemsTotal < order || latch)
                {
                    if (items.ElementAt(i).Value.TotalHours == 0)
                    {
                        continue;
                    }

                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * items.ElementAt(i).Value.TotalHours, GridUnitType.Star) });
                    spans.Add(items.ElementAt(i).Key.ProductName, 1);
                    addedColumns++;
                }
                else
                {
                    TimeSpan diff = itemsTotal - order;
                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * (items.ElementAt(i).Value - diff).TotalHours, GridUnitType.Star) });
                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * diff.TotalHours, GridUnitType.Star) });
                    latch = true;
                    spans.Add("order", addedColumns + 1);
                    spans.Add(items.ElementAt(i).Key.ProductName, 2);
                    addedColumns++;
                    diffStarts = addedColumns + 2;
                }
            }

            if (!latch)
            {
                // under-performance
                TimeSpan diff = order - itemsTotal;
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * diff.TotalHours, GridUnitType.Star) });
                spans.Add("order", addedColumns + 1);
                diffStarts = MainGrid.ColumnDefinitions.Count - 1;
            }

            //MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(unitsPerHour * remainingHours, GridUnitType.Star) });

            for (int i = 0; i < MainGrid.ColumnDefinitions.Count; i++)
            {
                Border element = new()
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new(1),
                };


                int s = 1;
                if (i == diffStarts)
                {
                    element = new()
                    {
                        Background = (Brush)Application.Current.Resources[latch ? "Green" : "Red"],
                        Margin = new(0),
                        CornerRadius = new(0),
                        Opacity = 0.7,
                        ToolTip = $"{(latch ? "Gained" : "Lost")} performance {remainingHours:0}h",
                    };

                    s = MainGrid.ColumnDefinitions.Count - diffStarts;
                }

                AddToMainGrid(element, i, MainGrid.RowDefinitions.Count - 1, s);
            }

            return spans;
        }

        private void DrawEvents(TimeSpan order, Dictionary<LatheManufactureOrderItem, TimeSpan> items, Dictionary<string, int> spanMap)
        {
            Border element;

            element = new()
            {
                Background = (Brush)Application.Current.Resources["Orange"],
                Margin = new(0),
                CornerRadius = new(0),
                Opacity = 0.7
            };

            AddToMainGrid(control: element, column: 1, row: 0);

            element = new()
            {
                Background = (Brush)Application.Current.Resources["Blue"],
                Margin = new(0),
                CornerRadius = new(0),
                Opacity = 0.7
            };
            element.ToolTip = $"[c{2} s{spanMap["order"]}] {order.TotalHours:0.00}";


            AddToMainGrid(control: element, column: 2, row: 0, colSpan: spanMap["order"]);

            int col = 2;
            for (int i = 0; i < items.Count; i++)
            {
                if (!spanMap.ContainsKey(items.ElementAt(i).Key.ProductName)) continue;

                element = new()
                {
                    Background = (Brush)Application.Current.Resources["Purple"],
                    Margin = new(0),
                    CornerRadius = new(0),
                    Opacity = 0.7
                };
                int spanOfItem = spanMap[items.ElementAt(i).Key.ProductName];
                element.ToolTip = $"[c{col} s{spanOfItem}] {items.ElementAt(i).Value.TotalHours:0.00}";

                AddToMainGrid(control: element, column: col, row: i + 1, colSpan: spanOfItem);
                col += spanOfItem;

            }
        }

        private void AddToMainGrid(UIElement control, int column, int row, int colSpan = 1, int rowSpan = 1)
        {
            MainGrid.Children.Add(control);
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            Grid.SetColumnSpan(control, colSpan);
            Grid.SetRowSpan(control, rowSpan);
        }
    }
}
