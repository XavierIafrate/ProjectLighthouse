using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class ScheduleTimeline : UserControl, INotifyPropertyChanged
    {
        List<TimelineSwimLane> swimLanes = new();
        Dictionary<string, Border> rowHeaders;
        private Border deadlineBorder;

        public ProductionSchedule Schedule
        {
            get { return (ProductionSchedule)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(ProductionSchedule), typeof(ScheduleTimeline), new PropertyMetadata(null, SetSchedule));


        public RescheduleItemCommand RescheduleCommand
        {
            get { return (RescheduleItemCommand)GetValue(RescheduleCommandProperty); }
            set { SetValue(RescheduleCommandProperty, value); }
        }

        public static readonly DependencyProperty RescheduleCommandProperty =
            DependencyProperty.Register("RescheduleCommand", typeof(RescheduleItemCommand), typeof(ScheduleTimeline), new PropertyMetadata(null, SetSchedule));


        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(ScheduleTimeline), new PropertyMetadata(null, SetSchedule));


        public ScheduleItem? SelectedItem
        {
            get { return (ScheduleItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ScheduleItem), typeof(ScheduleTimeline), new PropertyMetadata(null, SetSelectedItem));

        private static void SetSelectedItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleTimeline control) return;
            if (control.SelectedItem is not ScheduleItem item) return;
            List<ScheduleItem> highlighted = new();

            foreach (Optimisation optimisation in control.Schedule.Optimisations)
            {
                if (!optimisation.AffectedItems.Contains(item)) continue;

                foreach (ScheduleItem affectedItem in optimisation.AffectedItems)
                {
                    if (affectedItem == item) continue;
                    if (!highlighted.Contains(affectedItem))
                    {
                        highlighted.Add(affectedItem);
                    }
                }
            }

            foreach (TimelineSwimLane swimLane in control.swimLanes)
            {
                swimLane.HighlightedItems = highlighted;
            }

            control.AddColumnHeaderHighlights();
            control.AddRowHeaderHighlights();


            EnsureVisible(control, item);
        }

        private static void EnsureVisible(ScheduleTimeline control, ScheduleItem item)
        {
            if (item.StartDate == DateTime.MinValue) return;

            DateTime start = item.StartDate;
            DateTime end = item.EndsAt();

            if (item is LatheManufactureOrder order)
            {
                start = order.GetSettingStartDateTime();
                end = order.EndsAt();
            }

            double itemWidth = (end - start).TotalDays * control.ColumnWidth;
            double itemStarts = (start - (DateTime)control.MinDate!).TotalDays * control.ColumnWidth;
            double itemEnds = itemStarts + itemWidth;

            double viewportStart = control.TimelineScroller.HorizontalOffset;
            double viewportEnd = control.TimelineScroller.HorizontalOffset + control.TimelineScroller.ViewportWidth;


            if (itemStarts < viewportStart)
            {
                // Start obscured
                double dayOffset = (start - (DateTime)control.MinDate!).Days * control.ColumnWidth;
                control.TimelineScroller.ScrollToHorizontalOffset(dayOffset);
                return;
            }

            if (itemEnds > viewportEnd)
            {
                if (itemWidth > control.TimelineScroller.ViewportWidth) // won't fit - target start
                {
                    double dayOffset = (start - (DateTime)control.MinDate!).Days * control.ColumnWidth;
                    control.TimelineScroller.ScrollToHorizontalOffset(dayOffset);
                    return;
                }
                else // bring end into view
                {
                    double change = itemEnds - viewportEnd; // +ve
                    double newOffset = control.TimelineScroller.HorizontalOffset + change;
                    control.TimelineScroller.ScrollToHorizontalOffset(newOffset);
                    return;
                }
            }
        }

        private bool onHolidaysChangedSubscribed;
        private static void SetSchedule(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleTimeline control) return;
            if (control.Schedule == null) return;
            if (control.Schedule.MachineSchedules == null) return;
            if (control.SelectItemCommand == null) return;
            if (control.RescheduleCommand == null) return;

            if (!control.onHolidaysChangedSubscribed)
            {
                control.Schedule.OnHolidaysChanged += control.RedrawHolidays;
                control.onHolidaysChangedSubscribed = true;
            }

            control.DrawColumnHeaders();
            control.DrawRowHeaders();
            control.DrawSwimLanes();
        }

        private void RedrawHolidays(object sender, EventArgs e)
        {
            DrawColumnHeaders();
        }

        public int RowHeight
        {
            get { return (int)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        public static readonly DependencyProperty RowHeightProperty =
            DependencyProperty.Register("RowHeight", typeof(int), typeof(ScheduleTimeline), new PropertyMetadata(50, SetRowHeight));

        private static void SetRowHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleTimeline control) return;
            control.SetRowHeights();
        }

        private void SetRowHeights()
        {
            for (int i = 0; i < this.RowHeaderGrid.RowDefinitions.Count; i++)
            {
                this.RowHeaderGrid.RowDefinitions[i].Height = new(this.RowHeight);
            }

            for (int i = 0; i < this.MainGrid.RowDefinitions.Count; i++)
            {
                this.MainGrid.RowDefinitions[i].Height = new(this.RowHeight);
            }
        }

        public int ColumnWidth
        {
            get { return (int)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnWidthProperty =
           DependencyProperty.Register("ColumnWidth", typeof(int), typeof(ScheduleTimeline), new PropertyMetadata(50, SetColumnWidth));

        private static void SetColumnWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleTimeline control) return;
            control.SetColumnWidths();
        }

        private void SetColumnWidths()
        {
            for (int i = 0; i < this.ColumnHeaderGrid.ColumnDefinitions.Count; i++)
            {
                this.ColumnHeaderGrid.ColumnDefinitions[i].Width = new(this.ColumnWidth);
            }
        }

        public DateTime? MinDate
        {
            get { return (DateTime?)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime?), typeof(ScheduleTimeline), new PropertyMetadata(null, SetDates));


        public DateTime? MaxDate
        {
            get { return (DateTime?)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register("MaxDate", typeof(DateTime?), typeof(ScheduleTimeline), new PropertyMetadata(null, SetDates));

        public event PropertyChangedEventHandler PropertyChanged;

        private static void SetDates(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleTimeline control) return;

            control.MainGrid.Children.Clear();
            control.MainGrid.RowDefinitions.Clear();

            control.ColumnHeaderGrid.Children.Clear();
            control.ColumnHeaderGrid.ColumnDefinitions.Clear();

            if (control.MinDate is null) return;
            if (control.MaxDate is null) return;

            if (control.MinDate >= control.MaxDate) return;

            if (control.Schedule is not null)
            {
                control.DrawColumnHeaders();
                control.DrawSwimLanes();
            }
        }

        private void DrawColumnHeaders()
        {
            this.ColumnHeaderGrid.Children.Clear();
            this.ColumnHeaderGrid.ColumnDefinitions.Clear();
            this.ColumnHeaderGrid.RowDefinitions.Clear();

            this.ColumnHeaderGrid.RowDefinitions.Add(new() { Height = new(25) }); // Month
            this.ColumnHeaderGrid.RowDefinitions.Add(new() { Height = new(25) }); // ISO week
            this.ColumnHeaderGrid.RowDefinitions.Add(new() { Height = new(25) }); // Date

            AddToGrid(this.ColumnHeaderGrid, this.deadlineBorder, 0, 2);
            this.deadlineBorder.Visibility = Visibility.Collapsed;


            int daysSpan = ((TimeSpan)(MaxDate! - MinDate!)).Days + 1;

            string weekPartition = string.Empty;
            string monthPartition = string.Empty;

            for (int i = 0; i < daysSpan; i++)
            {
                this.ColumnHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(ColumnWidth, GridUnitType.Pixel) });
                DateTime date = ((DateTime)MinDate!).AddDays(i);
                ProcessDayHeader(i, date);
                weekPartition = ProcessWeekHeader(weekPartition, i, date);
                monthPartition = ProcessMonthHeader(monthPartition, i, date);
            }
        }

        private string ProcessMonthHeader(string monthPartition, int i, DateTime date)
        {
            string monthName = date.ToString("MMMM");
            if (monthName != monthPartition)
            {
                Border monthHeaderShading = new() { BorderBrush = Brushes.Black, BorderThickness = new(0, 1, 1, 1) };
                if (i == 0)
                {
                    monthHeaderShading.BorderThickness = new(1, 1, 1, 1);
                }

                TextBlock monthHeaderText = new()
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    Text = monthName,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                };

                AddToGrid(this.ColumnHeaderGrid, monthHeaderShading, i, 0, GetDaysRemainingInMonth(date));
                AddToGrid(this.ColumnHeaderGrid, monthHeaderText, i, 0, GetDaysRemainingInMonth(date));
                monthPartition = monthName;
            }

            return monthPartition;
        }

        private string ProcessWeekHeader(string weekPartition, int i, DateTime date)
        {
            string weekName = $"{ISOWeek.GetYear(date)}-{ISOWeek.GetWeekOfYear(date):00}";
            if (weekName != weekPartition)
            {
                Border weekHeaderShading = new() { BorderBrush = Brushes.Black, BorderThickness = new(0, 0, 1, 1) };
                if (i == 0)
                {
                    weekHeaderShading.BorderThickness = new(1, 0, 1, 1);
                }
                TextBlock weekHeaderText = new()
                {
                    Text = weekName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                };

                AddToGrid(this.ColumnHeaderGrid, weekHeaderShading, i, 1, GetDaysRemainingInWeek(date));
                AddToGrid(this.ColumnHeaderGrid, weekHeaderText, i, 1, GetDaysRemainingInWeek(date));

                weekPartition = weekName;
            }

            return weekPartition;
        }

        private void ProcessDayHeader(int i, DateTime date)
        {
            Border dayOutline = new() { BorderBrush = Brushes.Black, BorderThickness = new(0, 0, 1, 1) };

            if (i == 0)
            {
                dayOutline.BorderThickness = new(1, 0, 1, 1);
            }

            TextBlock headerText = new()
            {
                Text = date.ToString("dd"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
            };

            if (this.Schedule.Holidays.Contains(date))
            {
                dayOutline.Background = (Brush)Application.Current.Resources["OrangeFaded"];
                headerText.Foreground = (Brush)Application.Current.Resources["Orange"];
            }

            if (date.Date == DateTime.Today)
            {
                dayOutline.Background = (Brush)Application.Current.Resources["PurpleFaded"];
                headerText.Foreground = (Brush)Application.Current.Resources["Purple"];
            }

            AddToGrid(this.ColumnHeaderGrid, dayOutline, i, 2);
            AddToGrid(this.ColumnHeaderGrid, headerText, i, 2);
        }

        private static int GetDaysRemainingInWeek(DateTime date)
        {
            int week = ISOWeek.GetWeekOfYear(date);
            int i = 0;
            while (ISOWeek.GetWeekOfYear(date.AddDays(i)) == week)
            {
                i++;
            }

            return i;
        }

        private static int GetDaysRemainingInMonth(DateTime date)
        {
            int month = date.Month;
            int i = 0;
            while (date.AddDays(i).Month == month)
            {
                i++;
            }

            return i;
        }

        private void DrawRowHeaders()
        {
            this.RowHeaderGrid.Children.Clear();
            this.RowHeaderGrid.RowDefinitions.Clear();

            this.rowHeaders = new();

            for (int i = 0; i < this.Schedule.MachineSchedules.Count; i++)
            {
                this.RowHeaderGrid.RowDefinitions.Add(new() { Height = new(RowHeight) });
                MachineSchedule schedule = this.Schedule.MachineSchedules[i];

                this.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(RowHeight) });

                Border rowDiv = new()
                {
                    BorderThickness = new(0, 0, 0, 2),
                    BorderBrush = new LinearGradientBrush()
                    {
                        Opacity = 0.8,
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        GradientStops = new()
                        {
                            new GradientStop() { Offset =1, Color = Colors.Gray},
                            new GradientStop() { Offset =0, Color = Colors.Transparent},
                        }
                    }
                };

                if (i == 0)
                {
                    rowDiv.BorderThickness = new(0, 2, 0, 2);
                }

                StackPanel sp = new() { VerticalAlignment = VerticalAlignment.Center, Margin=new(5, 0, 0, 0) };

                TextBlock headerText = new()
                {
                    Text = schedule.Lathe.FullName,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                };

                TextBlock subHeaderText = new()
                {
                    Text = $"{schedule.Lathe.Make} {schedule.Lathe.Model}",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                sp.Children.Add(headerText);
                sp.Children.Add(subHeaderText);
                rowDiv.Child = sp;

                rowHeaders.Add(schedule.Lathe.Id, rowDiv);

                AddToGrid(this.RowHeaderGrid, rowDiv, 0, i);
            }
        }

        private void DrawSwimLanes()
        {
            this.MainGrid.Children.Clear();
            this.MainGrid.RowDefinitions.Clear();

            swimLanes = new();

            Binding colWidthBinding = new(nameof(ColumnWidth));
            Binding rowHeightBinding = new(nameof(RowHeight));
            Binding selectedItemBinding = new(nameof(SelectedItem));

            for (int i = 0; i < this.Schedule.MachineSchedules.Count; i++)
            {
                MachineSchedule schedule = this.Schedule.MachineSchedules[i];

                this.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(RowHeight) });
                TimelineSwimLane newSwimLane = new()
                {
                    Schedule = schedule,
                    MinDate = MinDate,
                    MaxDate = MaxDate,
                    SelectItemCommand = SelectItemCommand,
                    RescheduleCommand = RescheduleCommand,
                    MainTimeline = this,
                };

                newSwimLane.SetBinding(TimelineSwimLane.ColumnWidthProperty, colWidthBinding);
                newSwimLane.SetBinding(TimelineSwimLane.HeightProperty, rowHeightBinding);
                newSwimLane.SetBinding(TimelineSwimLane.SelectedItemProperty, selectedItemBinding);

                swimLanes.Add(newSwimLane);
                AddToGrid(this.MainGrid, newSwimLane, 0, i);
            }
        }

        private void AddToGrid(Grid grid, UIElement control, int column, int row, int colSpan = 1, int rowSpan = 1)
        {
            grid.Children.Add(control);
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            Grid.SetColumnSpan(control, colSpan);
            Grid.SetRowSpan(control, rowSpan);
        }

        public ScheduleTimeline()
        {
            InitializeComponent();

            this.deadlineBorder = new()
            {
                BorderThickness = new(0),
                Background = (Brush)Application.Current.Resources["RedFaded"],
                CornerRadius = new(0),
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false,
            };
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            LeftGrad.Visibility = scrollViewer.HorizontalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;

            RightGrad.Visibility = scrollViewer.HorizontalOffset + 1 > scrollViewer.ScrollableWidth
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        public void AddDragHighlights()
        {
            AddColumnHeaderHighlights();
            AddRowHeaderHighlights();
        }

        private void AddRowHeaderHighlights()
        {
            if (SelectedItem is null)
            {
                ClearRowHeaderHighlights();
                return;
            }
            
            foreach (MachineSchedule machineSchedule in Schedule.MachineSchedules)
            {
                if (machineSchedule.Lathe.CanRun(SelectedItem))
                {
                    Border rowHeader = rowHeaders[machineSchedule.Lathe.Id];
                    rowHeader.Background = Brushes.Transparent;
                }
                else
                {
                    Border rowHeader = rowHeaders[machineSchedule.Lathe.Id];
                    rowHeader.Background = (Brush)Application.Current.Resources["RedFaded"];
                }
            }
        }

        private void ClearRowHeaderHighlights()
        {
            foreach (MachineSchedule machineSchedule in Schedule.MachineSchedules)
            {
                Border rowHeader = rowHeaders[machineSchedule.Lathe.Id];
                rowHeader.Background = Brushes.Transparent;
            }
        }

        private void AddColumnHeaderHighlights()
        {
            if (SelectedItem is not LatheManufactureOrder order)
            {
                this.deadlineBorder.Visibility = Visibility.Collapsed;
                return;
            }


            DateTime deadline = order.GetStartDeadline().Date;

            if (deadline > MaxDate)
            {
                this.deadlineBorder.Visibility = Visibility.Collapsed;
                return;
            }

            DateTime startShading = deadline > (DateTime)MinDate! ? deadline : (DateTime)MinDate!;

            int colIndex = (startShading - (DateTime)MinDate!).Days;
            int span = ((DateTime)MaxDate! - startShading!).Days + 1;

            Grid.SetColumn(this.deadlineBorder, colIndex);
            Grid.SetColumnSpan(this.deadlineBorder, span);
            this.deadlineBorder.Visibility = Visibility.Visible;

        }

        public void RemoveDragHighlights()
        {
            ClearRowHeaderHighlights();
            ClearColumnHeaderHighlights();
        }

        private void ClearColumnHeaderHighlights()
        {
            this.deadlineBorder.Visibility = Visibility.Collapsed;
        }

        DateTime lastScroll = DateTime.MinValue;
        private void TimelineScroller_DragOver(object sender, DragEventArgs e)
        {
            if (DateTime.Now < lastScroll.AddMilliseconds(200))
            {
                return;
            }

            Point position = e.GetPosition(this.TimelineScroller);

            if (position.X > this.TimelineScroller.ViewportWidth - ColumnWidth)
            {
                this.TimelineScroller.ScrollToHorizontalOffset(this.TimelineScroller.HorizontalOffset + this.ColumnWidth);
                lastScroll = DateTime.Now;
                return;
            }

            if (position.X <  ColumnWidth)
            {
                this.TimelineScroller.ScrollToHorizontalOffset(this.TimelineScroller.HorizontalOffset - this.ColumnWidth);
                lastScroll = DateTime.Now;
                return;
            }

        }
    }
}
