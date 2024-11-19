using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class TimelineSwimLane : UserControl
    {
        List<TimelineOrder> timelineOrderControls = new();
        Border dropPreview;
        public ScheduleTimeline MainTimeline;

        private List<ScheduleItem> highlightedItems;
        public List<ScheduleItem> HighlightedItems
        {
            get { return highlightedItems; }
            set
            {
                highlightedItems = value;
                HighlightItems();
            }
        }

        private void HighlightItems()
        {
            foreach (TimelineOrder control in timelineOrderControls)
            {
                control.SetIsHighlighted(HighlightedItems.Contains(control.Item));
            }
        }

        public RescheduleItemCommand RescheduleCommand
        {
            get { return (RescheduleItemCommand)GetValue(RescheduleCommandProperty); }
            set { SetValue(RescheduleCommandProperty, value); }
        }

        public static readonly DependencyProperty RescheduleCommandProperty =
            DependencyProperty.Register("RescheduleCommand", typeof(RescheduleItemCommand), typeof(TimelineSwimLane), new PropertyMetadata(null));



        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(TimelineSwimLane), new PropertyMetadata(null, SetCommand));

        private static void SetCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineSwimLane control) return;
            control.PushCommand();
        }

        private void PushCommand()
        {
            foreach (TimelineOrder control in timelineOrderControls)
            {
                control.SelectCommand = SelectItemCommand;
            }
        }


        public ScheduleItem? SelectedItem
        {
            get { return (ScheduleItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ScheduleItem), typeof(TimelineSwimLane), new PropertyMetadata(null));

        public DateTime? MinDate
        {
            get { return (DateTime?)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime?), typeof(TimelineSwimLane), new PropertyMetadata(null, SetDateSpan));


        public DateTime? MaxDate
        {
            get { return (DateTime?)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register("MaxDate", typeof(DateTime?), typeof(TimelineSwimLane), new PropertyMetadata(null, SetDateSpan));

        private static void SetDateSpan(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineSwimLane control) return;

            control.TryDraw(null, EventArgs.Empty);
        }

        public int ColumnWidth
        {
            get { return (int)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(int), typeof(TimelineSwimLane), new PropertyMetadata(50, SetColumnWidth));

        private static void SetColumnWidth(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineSwimLane control) return;
            control.SetColumnWidth();
        }

        private void SetColumnWidth()
        {
            for (int i = 0; i < this.MainGrid.ColumnDefinitions.Count; i++)
            {
                this.MainGrid.ColumnDefinitions[i].Width = new(this.ColumnWidth);
            }
        }

        private void TryDraw(object sender, System.EventArgs e)
        {
            this.MainGrid.Children.Clear();
            this.MainGrid.ColumnDefinitions.Clear();

            if (this.MinDate is null) return;
            if (this.MaxDate is null) return;
            if (this.Schedule is null) return;
            if (this.MaxDate <= this.MinDate) return;

            int daysSpan = ((TimeSpan)(MaxDate - MinDate)).Days + 1;
            Dictionary<DateTime, int> colMap = new();
            for (int i = 0; i < daysSpan; i++)
            {
                DateTime date = ((DateTime)MinDate).Date.AddDays(i);
                this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new(ColumnWidth) });
                colMap.Add(date, i);

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    Border weekendShading = new() { Background = (Brush)Application.Current.Resources["Surface"] };
                    AddToGrid(this.MainGrid, weekendShading, i, 0);
                }

                if (Schedule.Holidays.Contains(date))
                {
                    Border holidayShading = new() { Background = (Brush)Application.Current.Resources["OrangeFaded"] };
                    AddToGrid(this.MainGrid, holidayShading, i, 0);
                }
            }

            DrawScheduleItems(colMap);

            AddToGrid(this.MainGrid, this.dropPreview, 0, 0);
            PushCommand();
        }

        private void DrawScheduleItems(Dictionary<DateTime, int> colMap)
        {
            Binding colWidthBinding = new(nameof(ColumnWidth));
            Binding selectItemCommandBinding = new(nameof(SelectItemCommand));
            Binding selectedItemBinding = new(nameof(SelectedItem));
            Binding minDateBinding = new(nameof(MinDate));
            Binding maxDateBinding = new(nameof(MaxDate));

            List<ScheduleItem> itemsInWindow = Schedule.GetItems(MinDate, MaxDate);
            for (int i = 0; i < itemsInWindow.Count; i++)
            {
                ScheduleItem item = itemsInWindow[i];
                TimelineOrder orderControl = new()
                {
                    Item = item,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                };

                orderControl.SetBinding(TimelineOrder.MinDateProperty, minDateBinding);
                orderControl.SetBinding(TimelineOrder.MaxDateProperty, maxDateBinding);
                orderControl.SetBinding(TimelineOrder.ColumnWidthProperty, colWidthBinding);
                orderControl.SetBinding(TimelineOrder.SelectCommandProperty, selectItemCommandBinding);
                orderControl.SetBinding(TimelineOrder.SelectedItemProperty, selectedItemBinding);


                DateTime startDate = item.StartDate;
                DateTime endDate = item.EndsAt();
                if (item is LatheManufactureOrder order)
                {
                    startDate = order.GetSettingStartDateTime();
                    endDate = order.EndsAt();
                }

                if (startDate < MinDate)
                {
                    startDate = (DateTime)MinDate;
                }

                int span = Convert.ToInt32((endDate.Date - startDate.Date).TotalDays) + 1;

                AddToGrid(this.MainGrid, orderControl, colMap[startDate.Date], 0, span);
                timelineOrderControls.Add(orderControl);
            }
        }

        public MachineSchedule Schedule
        {
            get { return (MachineSchedule)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(MachineSchedule), typeof(TimelineSwimLane), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineSwimLane control) return;
            control.TryDraw(null, EventArgs.Empty);
            control.Schedule.OnScheduleUpdated += control.TryDraw;
            control.Schedule.OnHolidaysUpdated += control.TryDraw;
        }

        private static void AddToGrid(Grid grid, UIElement control, int column, int row, int colSpan = 1, int rowSpan = 1)
        {
            grid.Children.Add(control);
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            Grid.SetColumnSpan(control, colSpan);
            Grid.SetRowSpan(control, rowSpan);
        }

        public TimelineSwimLane()
        {
            InitializeComponent();
            this.dropPreview = new()
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new(4),
                CornerRadius = new(4),
                Visibility = Visibility.Collapsed,
                Opacity = 0.6,
                IsHitTestVisible = false,
            };
        }

        private void MainGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (RescheduleCommand is null) return;
            if (e.Data.GetData(typeof(TimelineOrder)) is not TimelineOrder orderControl) return;

            MainTimeline.AddDragHighlights();

            if (!this.Schedule.Machine.CanRun(orderControl.Item))
            {
                return;
            }

            this.dropPreview.Margin = orderControl.ItemButton.Margin;


            int colSpan = (orderControl.Item.EndsAt() - orderControl.Item.StartDate.Date).Days + 1;
            if (orderControl.Item is LatheManufactureOrder order)
            {
                colSpan = (order.EndsAt().AddHours(order.TimeToSet) - order.StartDate.Date).Days + 1;
            }

            Grid.SetColumnSpan(this.dropPreview, colSpan);
            this.dropPreview.Background = orderControl.BackgroundBrush;
            this.dropPreview.BorderBrush = orderControl.ForegroundBrush;
            this.dropPreview.Visibility = Visibility.Visible;
        }

        private void MainGrid_DragLeave(object sender, DragEventArgs e)
        {
            this.dropPreview.Visibility = Visibility.Collapsed;
            MainTimeline.RemoveDragHighlights();
        }

        private void MainGrid_DragOver(object sender, DragEventArgs e)
        {
            if (RescheduleCommand is null) return;

            Point position = e.GetPosition(this.MainGrid);
            int todayIndex = (DateTime.Today - (DateTime)MinDate!).Days;
            int colIndex = (int)Math.Floor(position.X / ColumnWidth);
            Grid.SetColumn(this.dropPreview, Math.Max(colIndex, todayIndex));
        }

        private void MainGrid_Drop(object sender, DragEventArgs e)
        {
            if (RescheduleCommand is null) return;
            if (e.Data.GetData(typeof(TimelineOrder)) is not TimelineOrder orderControl) return;
            this.dropPreview.Visibility = Visibility.Collapsed;
            MainTimeline.RemoveDragHighlights();

            if (!this.Schedule.Machine.CanRun(orderControl.Item))
            {
                return;
            }

            int colIndex = Grid.GetColumn(this.dropPreview);

            DateTime targetDate = ((DateTime)MinDate!).AddDays(colIndex);
            if (orderControl.Item.StartDate == DateTime.MinValue)
            {
                targetDate = targetDate.ChangeTime(12, 0, 0, 0);
            }
            else
            {
                targetDate = targetDate.ChangeTime(orderControl.Item.StartDate.Hour, orderControl.Item.StartDate.Minute, 0, 0);

            }


            if (targetDate.Date < DateTime.Today) return;

            if (targetDate != orderControl.Item.StartDate || Schedule.Machine.Id != orderControl.Item.AllocatedMachine)
            {
                ScheduleItem item = orderControl.Item;
                RescheduleInformation rescheduleParams = new(item, this.Schedule.Machine.Id, targetDate);

                RescheduleCommand?.Execute(rescheduleParams);
            }
        }

        private void MainGrid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }
    }
}
