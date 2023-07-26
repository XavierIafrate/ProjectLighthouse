using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class TimelineView : UserControl
    {
        private List<BarStock> bars;

        public List<DateTime> Holidays
        {
            get { return (List<DateTime>)GetValue(HolidaysProperty); }
            set { SetValue(HolidaysProperty, value); }
        }

        public static readonly DependencyProperty HolidaysProperty =
            DependencyProperty.Register("Holidays", typeof(List<DateTime>), typeof(TimelineView), new PropertyMetadata(null, SetValues));

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
            bars = DatabaseHelper.Read<BarStock>();
        }

        private void Draw()
        {
            this.MainGrid.Children.Clear();
            this.MainGrid.ColumnDefinitions.Clear();
            this.MainGrid.RowDefinitions.Clear();

            if (ActiveItems is null) return;
            if (ActiveItems.Count == 0) return;
            if (Holidays is null) return;

            DateTime minDate = DateTime.Today.AddDays(-3);
            DateTime maxDate = ActiveItems.Max(x => x.EndsAt()).Date.AddDays(2);

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
                else if (item is MachineService)
                {
                    itemsToDraw.Add(item);
                }
            }

            DrawGrid(minDate, daysSpan, itemsToDraw, Holidays);
            DrawEvents(minDate, itemsToDraw);
        }

        private void DrawGrid(DateTime minDate, int daysSpan, List<ScheduleItem> items, List<DateTime> holidays)
        {
            string[] rows = items.Select(x => x.AllocatedMachine)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            // Rows
            MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            for (int i = 0; i < rows.Length; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });

                TextBlock rowHeader = new()
                {
                    Text = rows[i],
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new(5,0,5,0),
                };

                AddToMainGrid(rowHeader, 0, i +1);
            }

            // Columns
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            for (int i = 0; i < daysSpan; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(52) });
                DateTime date = minDate.AddDays(i);

                TextBlock colHeader = new()
                {
                    Text = $"{date:dd}",
                    Margin=new(5),
                    FontWeight=FontWeights.SemiBold,
                    HorizontalAlignment= HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Bottom,
                };

                if (date == DateTime.Today)
                {
                    colHeader.Foreground = (Brush)Application.Current.Resources["Purple"];
                    colHeader.TextDecorations = new() { TextDecorations.Underline };
                    colHeader.FontWeight = FontWeights.Bold;
                }

                if (holidays.Contains(date))
                {
                    Border bg = new() { Background = (Brush)App.Current.Resources["PurpleFaded"] };
                    AddToMainGrid(bg, column: i+1, row: 1, colSpan: 1, rowSpan: rows.Length);
                }
                else if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    Border bg = new() { Background = Brushes.LightGray };
                    AddToMainGrid(bg, column: i+1, row: 1, colSpan: 1, rowSpan: rows.Length);
                }
                AddToMainGrid(colHeader, i + 1, 0);
            }
        }

        private void DrawEvents(DateTime minDate, List<ScheduleItem> items)
        {
            string[] rows = items.Select(x => x.AllocatedMachine).Distinct().OrderBy(x => x).ToArray();


            items = items.OrderBy(x => x.StartDate).ToList();

            List<ScheduleItem> latheItems;

            foreach (string row in rows)
            {
                BarStock bar = null;

                latheItems = items.Where(x => x.AllocatedMachine == row).OrderBy(x => x.StartDate).ToList();

                for (int i = 0; i < latheItems.Count; i++)
                {
                    ScheduleItem item = latheItems[i];

                    if (item is MachineService service)
                    {
                        Border serviceMarker = new()
                        {
                            Margin = new(service.StartDate.Hour * 2, 2, 50 - (service.EndsAt().Hour * 2), 2),
                            Background = (Brush)Application.Current.Resources["Orange"],
                            CornerRadius = new(5),
                            Opacity = 0.7,
                            ToolTip="Maintenance"
                        };

                        AddToMainGrid(
                            control: serviceMarker, 
                            column: (int)(service.StartDate.Date - minDate.Date).TotalDays + 1, 
                            row: Array.IndexOf(rows, row) + 1, 
                            colSpan: (int)Math.Ceiling((service.EndsAt() - service.StartDate.Date).TotalDays), 
                            rowSpan: 1);   

                        continue;
                    }

                    if (item is not LatheManufactureOrder order)
                    {
                        continue;
                    }
                    if (i == 0)
                    {
                        bar = bars.Find(x => x.Id == order.BarID);
                    }


                    BarStock orderBar = bars.Find(x => x.Id == order.BarID);
                    
                    if (bar is null) return;
                    if (orderBar is null) return;

                    DateTime startDate = order.StartDate;
                    DateTime endsAt = order.EndsAt();
                    int startColIndex = (int)(startDate.Date - minDate.Date).TotalDays;
                    int span = (int)Math.Ceiling((endsAt - startDate.Date).TotalDays);
                    int startMargin = 14; 
                    int endMargin = 50 - (endsAt.Hour * 2);
                    bool startClipped = false;

                    int iRow = Array.IndexOf(rows, row) + 1;

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
                        span = Math.Max(span, 3); 
                        borderBrush = (Brush)Application.Current.Resources[order.IsComplete ? "OnSurface" : "Purple"];
                        endMargin = 38;
                    }

                    Border element = new()
                    {
                        Margin = new(startMargin, 2, endMargin, 2),
                        Background=borderBrush,
                        CornerRadius= startClipped? new CornerRadius(0,5,5,0) : new(5),
                        Opacity = 0.7
                    };

                    TextBlock itemTitle = new()
                    {
                        Margin=new(28,0,4,0),
                        Text = order.Name + " " + orderBar.Size.ToString("0") + (orderBar.IsHexagon? "H" :"R"),
                        FontSize=10, 
                        Background=borderBrush,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground=Brushes.White,
                        HorizontalAlignment=HorizontalAlignment.Left
                    };

                    if (bar.Size == orderBar.Size && bar.IsHexagon == orderBar.IsHexagon)
                    {
                        itemTitle.Text += "*";
                    }

                    AddToMainGrid(element, startColIndex, iRow, span);
                    AddToMainGrid(itemTitle, startColIndex, iRow, span);

                    bar = orderBar;
                
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
