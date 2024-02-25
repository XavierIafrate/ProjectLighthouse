using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class TimelineOrder : UserControl
    {
        public Brush BackgroundBrush = (Brush)Application.Current.Resources["OrangeFaded"];
        public Brush ForegroundBrush = (Brush)Application.Current.Resources["Orange"];

        public Brush CheckedBackgroundBrush = (Brush)Application.Current.Resources["Orange"];
        public Brush CheckedForegroundBrush = (Brush)Application.Current.Resources["OnOrange"];


        public bool IsHighlighted;
        public void SetIsHighlighted(bool isHighlighted)
        {
            this.IsHighlighted = isHighlighted;
            HighlightBorder.Visibility = isHighlighted
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime), typeof(TimelineOrder), new PropertyMetadata(DateTime.MinValue, SetValues));

        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(TimelineOrder), new PropertyMetadata(DateTime.MaxValue, SetValues));


        public ScheduleItem Item
        {
            get { return (ScheduleItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ScheduleItem), typeof(TimelineOrder), new PropertyMetadata(null, SetItem));

        public SelectScheduleItemCommand SelectCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(SelectScheduleItemCommand), typeof(TimelineOrder), new PropertyMetadata(null));


        public ScheduleItem? SelectedItem
        {
            get { return (ScheduleItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ScheduleItem), typeof(TimelineOrder), new PropertyMetadata(null, SetIsSelected));

        private static void SetIsSelected(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineOrder control) return;

            if (control.SelectedItem == null && (control.ItemButton.IsChecked ?? false))
            {
                control.ItemButton.IsChecked = false;
                return;
            }

            if (control.SelectedItem == null) return;

            control.ItemButton.IsChecked = control.SelectedItem == control.Item;
        }

        public int ColumnWidth
        {
            get { return (int)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(int), typeof(TimelineOrder), new PropertyMetadata(50, SetValues));

        private void ApplyItemMargin()
        {
            if (MaxDate == DateTime.MaxValue) return;
            if (MinDate == DateTime.MinValue) return;


            DateTime startDraw = this.Item.StartDate;
            DateTime endDraw = this.Item.EndsAt();

            if (this.Item is LatheManufactureOrder order)
            {
                startDraw = order.GetSettingStartDateTime();
                endDraw = order.EndsAt(); // handles research orders
            }

            double startHour = startDraw.Hour;
            double startMargin = startHour / 24 * ColumnWidth;

            double endHour = endDraw.Hour;
            double endMargin = ColumnWidth - endHour / 24 * ColumnWidth;

            this.LeftClippingBorder.Visibility = Visibility.Collapsed;
            this.RightClippingBorder.Visibility = Visibility.Collapsed;

            if (this.MinDate > startDraw)
            {
                startMargin = 0;
                this.LeftClippingBorder.Visibility = Visibility.Visible;
            }


            if (this.MaxDate.AddDays(1) < endDraw)
            {
                endMargin = 0;
                this.RightClippingBorder.Visibility = Visibility.Visible;
            }

            this.ItemButton.Margin = new(startMargin, 0, endMargin, 0);
        }


        private static void SetItem(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineOrder control) return;
            if (control.Item is null) return;

            control.Item.OnAdvisoriesChanged += control.SetAdvisoryBadgeVis;
            control.Item.OnWarningsChanged += control.SetWarningBadgeVis;

            control.Draw();
        }


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimelineOrder control) return;
            if (control.Item is null) return;

            control.Draw();
        }

        private void Draw()
        {
            if (MaxDate == DateTime.MaxValue) return;
            if (MinDate == DateTime.MinValue) return;
            this.OrderNameTextBlock.Text = this.Item.Name;

            double absoluteSetting = 0;
            double relativeBreakdown = 0;
            bool runningVisible = true;

            if (this.Item is LatheManufactureOrder order)
            {
                this.StatusTextBlock.Text = order.State.ToString();
                this.StatusTextBlock.Text = order.BarID;
                this.StatusTextBlock.Visibility = Visibility.Visible;

                DateTime effectiveMaxDate = MaxDate.AddDays(1);

                TimeSpan visibleSetting;
                TimeSpan visibleRuntime;

                DateTime settingStarts = order.GetSettingStartDateTime();
                DateTime runningStarts = order.StartDate;
                DateTime runningEnds = order.EndsAt();

                runningEnds = runningEnds <= effectiveMaxDate ? runningEnds : effectiveMaxDate;
                runningStarts = runningStarts <= effectiveMaxDate ? runningStarts : effectiveMaxDate;
                // Wont be drawn if setting is greater than MaxDate

                settingStarts = settingStarts >= MinDate ? settingStarts : MinDate;
                runningStarts = runningStarts >= MinDate ? runningStarts : MinDate;
                // Wont be drawn if end is less than MinDate

                visibleRuntime = runningEnds - runningStarts;
                visibleSetting = runningStarts - settingStarts;

                runningVisible = visibleRuntime != TimeSpan.Zero;
                absoluteSetting = ColumnWidth * visibleSetting.TotalHours / 24;
                relativeBreakdown = (double)order.Breakdowns.Sum(x => x.TimeElapsed) / order.TimeToComplete;

                this.SetBrushes(order.State);
                if (order.IsResearch)
                {
                    if (!order.IsComplete)
                    {
                        this.BackgroundBrush = (Brush)Application.Current.Resources["PurpleFaded"];
                        this.ForegroundBrush = (Brush)Application.Current.Resources["Purple"];
                    }
                    this.setting_border.Visibility = Visibility.Collapsed;
                    absoluteSetting = 0;
                    relativeBreakdown = 0;
                }
            }

            this.ItemButton.Background = this.BackgroundBrush;
            this.OrderNameTextBlock.Foreground = this.ForegroundBrush;
            this.StatusTextBlock.Foreground = this.ForegroundBrush;
            this.HatchingPath.Stroke = this.ForegroundBrush;
            this.BackgroundBorder.Background = this.BackgroundBrush;

            if (this.ItemButton.IsChecked ?? false)
            {
                this.OrderNameTextBlock.Foreground = CheckedForegroundBrush;
                this.StatusTextBlock.Foreground = CheckedForegroundBrush;
                this.HatchingPath.Stroke = CheckedForegroundBrush;
                this.BackgroundBorder.Background = CheckedBackgroundBrush;
            }

            this.OrderGrid.ColumnDefinitions[0].Width = new(absoluteSetting, GridUnitType.Pixel);
            this.OrderGrid.ColumnDefinitions[1].Width = new(runningVisible ? relativeBreakdown : 0, GridUnitType.Star);
            this.OrderGrid.ColumnDefinitions[2].Width = new(runningVisible ? 1 : 0, GridUnitType.Star);

            this.SequenceLockToggleButton.IsChecked = this.Item.ScheduleLockData is not null;

            SetAdvisoryBadgeVis(this, EventArgs.Empty);
            SetWarningBadgeVis(this, EventArgs.Empty);
            ApplyItemMargin();
        }

        private void SetBrushes(OrderState state)
        {
            switch (state)
            {
                case OrderState.Problem:
                    BackgroundBrush = (Brush)Application.Current.Resources["RedFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["Red"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["Red"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["OnRed"];
                    break;
                case OrderState.Ready:
                    BackgroundBrush = (Brush)Application.Current.Resources["GreenFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["Green"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["Green"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["OnGreen"];
                    break;
                case OrderState.Prepared:
                    BackgroundBrush = (Brush)Application.Current.Resources["TealFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["Teal"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["Teal"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["OnTeal"];
                    break;
                case OrderState.Running:
                    BackgroundBrush = (Brush)Application.Current.Resources["BlueFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["Blue"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["Blue"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["OnBlue"];
                    break;
                case OrderState.Complete:
                    BackgroundBrush = (Brush)Application.Current.Resources["OnBackgroundFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["OnBackground"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["OnBackground"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["Background"];
                    break;
                default:
                    BackgroundBrush = (Brush)Application.Current.Resources["OrangeFaded"];
                    ForegroundBrush = (Brush)Application.Current.Resources["Orange"];
                    CheckedBackgroundBrush = (Brush)Application.Current.Resources["Orange"];
                    CheckedForegroundBrush = (Brush)Application.Current.Resources["OnOrange"];
                    break;
            };
        }

        public TimelineOrder()
        {
            InitializeComponent();
            //SequenceLockToggleButton.Visibility = App.CurrentUser.Role >= UserRole.Scheduling
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;
        }

        private void ItemButton_Checked(object sender, RoutedEventArgs e)
        {
            this.OrderNameTextBlock.Foreground = CheckedForegroundBrush;
            this.StatusTextBlock.Foreground = this.HatchingPath.Stroke = CheckedForegroundBrush;
            ;
            this.HatchingPath.Stroke = CheckedForegroundBrush;
            this.BackgroundBorder.Background = CheckedBackgroundBrush;


            if (this.SelectCommand is null)
            {
                MessageBox.Show("Command is null");
                return;
            }

            if (this.SelectedItem == this.Item) return;

            this.SelectCommand?.Execute(this.Item);
        }

        private void ItemButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.OrderNameTextBlock.Foreground = ForegroundBrush;
            this.StatusTextBlock.Foreground = ForegroundBrush;
            this.HatchingPath.Stroke = ForegroundBrush;
            this.BackgroundBorder.Background = BackgroundBrush;

            if (this.SelectedItem is null) return;
            if (this.SelectedItem == this.Item)
            {
                this.SelectCommand?.Execute(null);
            }
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.ItemButton.IsChecked ?? false) return;
            if (this.Item is LatheManufactureOrder order)
            {
                if (order.State >= OrderState.Running)
                {
                    return;
                }
            }

            DragDrop.DoDragDrop(this,
                                 this,
                                 DragDropEffects.Move);
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OrderGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (OrderGrid.ActualWidth - OrderGrid.ColumnDefinitions[0].ActualWidth < 100)
            {
                MetadataStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MetadataStackPanel.Visibility = Visibility.Visible;
            }
        }


        private void SetWarningBadgeVis(object sender, EventArgs e)
        {
            this.WarningBadge.Visibility = this.Item.Warnings.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetAdvisoryBadgeVis(object sender, EventArgs e)
        {
            this.AdvisoryBadge.Visibility = this.Item.Advisories.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        }
    }
}
