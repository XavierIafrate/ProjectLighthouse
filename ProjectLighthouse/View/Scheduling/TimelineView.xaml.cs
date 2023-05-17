using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class TimelineView : UserControl
    {
        public List<ScheduleItem> ActiveItems
        {
            get { return (List<ScheduleItem>)GetValue(ActiveItemsProperty); }
            set { SetValue(ActiveItemsProperty, value); }
        }

        public static readonly DependencyProperty ActiveItemsProperty =
            DependencyProperty.Register("ActiveItems", typeof(List<ScheduleItem>), typeof(TimelineView), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineView control) return;

            control.Draw();
        }

        public TimelineView()
        {
            InitializeComponent();
        }

        private void Draw()
        {
            this.MainGrid.Children.Clear();
            this.MainGrid.ColumnDefinitions.Clear();
            this.MainGrid.RowDefinitions.Clear();

            if (ActiveItems is null) return;

            DateTime minDate = DateTime.Today.AddDays(-3);
            DateTime maxDate = ActiveItems.Max(x => x.EndsAt()).Date.AddDays(1);

            int daysSpan = (int)(maxDate - minDate).TotalDays;

            List<ScheduleItem> itemsToDraw = new();

            for (int i = 0; i < ActiveItems.Count; i++)
            {
                ScheduleItem item = ActiveItems[i];

                if (item.EndsAt() < minDate || item.StartDate > maxDate || string.IsNullOrEmpty(item.AllocatedMachine))
                {
                    continue;
                }

                if (item is LatheManufactureOrder order)
                {
                    if (order.State == OrderState.Cancelled)
                    {
                        continue;
                    }

                    itemsToDraw.Add(item);
                }
            }

            DrawGrid(minDate, daysSpan, itemsToDraw);
            DrawEvents(minDate, itemsToDraw);
        }

        private void DrawGrid(DateTime minDate, int daysSpan, List<ScheduleItem> items)
        {
            string[] rows = items.Select(x => x.AllocatedMachine)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            // Rows
            MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

            for (int i = 0; i < rows.Length; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

                TextBlock rowHeader = new()
                {
                    Text = rows[i],
                    VerticalAlignment = VerticalAlignment.Center,
                };

                AddToMainGrid(rowHeader, 0, i +1);
            }

            // Columns
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });

            for (int i = 0; i < daysSpan; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(52) });
                DateTime date = minDate.AddDays(i);

                TextBlock colHeader = new()
                {
                    Text = $"{date:dd}",
                };

                AddToMainGrid(colHeader, i+1, 0);
            }
        }

        private void DrawEvents(DateTime minDate, List<ScheduleItem> items)
        {
            string[] rows = items.Select(x => x.AllocatedMachine).Distinct().OrderBy(x => x).ToArray();

            for (int i = 0; i < items.Count; i++)
            {
                ScheduleItem item = items[i];

                if (item is LatheManufactureOrder order)
                {
                    DateTime startDate = order.StartDate;
                    DateTime endsAt = order.EndsAt();
                    int startColIndex = (int)(startDate.Date - minDate.Date).TotalDays;
                    int span = (int)Math.Ceiling((endsAt - startDate.Date).TotalDays);
                    int startMargin = 26; 
                    int endMargin = 2;
                    bool startClipped = false;

                    int row = Array.IndexOf(rows, order.AllocatedMachine) + 1;

                    if (startColIndex < 0)
                    {
                        // starts before window
                        startClipped = true;
                        span += startColIndex;
                        startColIndex = 0;
                        startMargin = 0;
                    }

                    // starts in column 1
                    startColIndex++;

                    Brush borderBrush = order.State switch
                    {
                        OrderState.Problem => (Brush)Application.Current.Resources["Red"],
                        OrderState.Ready => (Brush)Application.Current.Resources["Green"],
                        OrderState.Prepared => (Brush)Application.Current.Resources["GreenDark"],
                        OrderState.Running => (Brush)Application.Current.Resources["Blue"],
                        _ => (Brush)Application.Current.Resources["OnBackground"],
                    };

                    if (order.IsResearch)
                    {
                        span = Math.Max(span, 2); 
                        borderBrush = (Brush)Application.Current.Resources["Purple"];
                    }

                    Border element = new()
                    {
                        Margin = new(startMargin, 2, endMargin, 2),
                        Background=borderBrush,
                        CornerRadius= startClipped? new CornerRadius(0,5,5,0) : new(5),
                        Opacity = 0.6
                    };

                    TextBlock itemTitle = new()
                    {
                        Margin=new(28,0,4,0),
                        Text = order.Name,
                        FontSize=10, 
                        VerticalAlignment= VerticalAlignment.Center,
                    };

                    AddToMainGrid(element, startColIndex, row, span);
                    AddToMainGrid(itemTitle, startColIndex, row, span);
                }
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
