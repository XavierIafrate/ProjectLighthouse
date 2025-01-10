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
    public partial class BreakdownEditor : UserControl
    {
        private List<BreakdownCode> breakdownCodes;

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(BreakdownEditor), new PropertyMetadata(false, SetCanEdit));

        private static void SetCanEdit(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BreakdownEditor control)
            {
                return;
            }

            control.AddNewBreakdownControls.Visibility = control.CanEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(BreakdownEditor), new PropertyMetadata(null, ReDraw));

        private static void ReDraw(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not BreakdownEditor control) return;
            if (control.Order is null) return;
            control.Draw();
            DateTime now = new();
            bool runningNowBySchedule = now > control.Order.StartDate && now < control.Order.EndsAt();

            control.NewBreakdown.BreakdownEnded = runningNowBySchedule ? now.ChangeTime(now.Hour, 0, 0, 0) : control.Order.EndsAt().ChangeTime(control.Order.EndsAt().Hour, 0, 0, 0);
            control.NewBreakdown.BreakdownStarted = control.NewBreakdown.BreakdownEnded.AddHours(-1) > control.Order.StartDate ? control.NewBreakdown.BreakdownEnded.AddHours(-1) : control.Order.StartDate;
        }

        private MachineBreakdown newBreakdown;

        public MachineBreakdown NewBreakdown
        {
            get { return newBreakdown; }
            set { newBreakdown = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BreakdownEditor()
        {
            InitializeComponent();
            NewBreakdown = new();

            NewBreakdown.PropertyChanged += OnNewBreakdownPropertyChanged;

            breakdownCodes = DatabaseHelper.Read<BreakdownCode>();
            this.BreakdownCodes_ComboBox.ItemsSource = this.breakdownCodes;
            if (this.breakdownCodes.Count > 0)
            {
                this.BreakdownCodes_ComboBox.SelectedIndex = 0;
            }

            if (breakdownCodes.Count > 0)
            {
                NewBreakdown.BreakdownCode = breakdownCodes.First().Id;
            }
        }

        private void AddBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (BreakdownCodes_ComboBox.SelectedValue is not BreakdownCode selectedBreakdownCode)
            {
                return;
            }

            if (!NewBreakdown.ValidateOverlap(Order.Breakdowns))
            {
                MessageBox.Show("New record overlaps time of other breakdown record", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.OrderName = Order.Name;
            NewBreakdown.CreatedAt = DateTime.Now;
            NewBreakdown.CreatedBy = App.CurrentUser.UserName;
            NewBreakdown.BreakdownCode = selectedBreakdownCode.Id;
            NewBreakdown.BreakdownMeta = selectedBreakdownCode;

            Order.Breakdowns = Order.Breakdowns.Append((MachineBreakdown)NewBreakdown.Clone()).ToList();

            NewBreakdown.PropertyChanged -= OnNewBreakdownPropertyChanged;
            NewBreakdown = new();
            NewBreakdown.PropertyChanged += OnNewBreakdownPropertyChanged;

            DateTime now = DateTime.Now;
            NewBreakdown.BreakdownEnded = now.ChangeTime(now.Hour, 0, 0, 0);
            NewBreakdown.BreakdownStarted = NewBreakdown.BreakdownEnded.AddHours(-1);

            if (breakdownCodes.Count > 0)
            {
                NewBreakdown.BreakdownCode = breakdownCodes.First().Id;
            }
        }

        private void OnNewBreakdownPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Draw();
        }

        private void BreakdownViewGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            if (Order is null) return;
            if (!breakdownOverlayGrid.IsLoaded || !BreakdownViewGrid.IsLoaded) return;

            breakdownOverlayGrid.Children.Clear();
            newBreakdownOverlayGrid.Children.Clear();


            DateTime start = Order.GetSettingStartDateTime();
            DateTime end = Order.EndsAt();
            TimeSpan overrun = TimeSpan.Zero;

            if (NewBreakdown.BreakdownEnded > Order.EndsAt())
            {
                overrun = NewBreakdown.BreakdownEnded - Order.EndsAt();
                end = NewBreakdown.BreakdownEnded;
            }


            TimeSpan coverage = end - start;

            if (coverage.TotalMinutes == 0) return;

            double pxPerMinute = BreakdownViewGrid.ActualWidth / coverage.TotalMinutes;

            BreakdownViewGrid.ColumnDefinitions[0].Width = new(pxPerMinute * Order.TimeToSet * 60);
            BreakdownViewGrid.ColumnDefinitions[2].Width = new(pxPerMinute * overrun.TotalMinutes);


            testTextBlock.Text = $"{coverage.TotalMinutes:0} mins {Order.TimeToSet * 60:0} setting mins";
            testTextBlock.Text += Environment.NewLine + $"Order running {Order.StartDate} to {Order.EndsAt()}";
            testTextBlock.Text += Environment.NewLine + $"pxPerMin: {pxPerMinute:0.000}";

            for (int i = 0; i < Order.Breakdowns.Count; i++)
            {
                MachineBreakdown breakdown = Order.Breakdowns[i];
                double lMargin = (breakdown.BreakdownStarted - Order.StartDate).TotalMinutes * pxPerMinute;
                double rMargin = (Order.EndsAt() - breakdown.BreakdownEnded).TotalMinutes * pxPerMinute;
                double width = (breakdown.BreakdownEnded - breakdown.BreakdownStarted).TotalMinutes; // Doesn't add up!
                width *= pxPerMinute;

                DisplayBreakdown breakdownGraphic = new()
                {
                    CodeText = breakdown.BreakdownCode,
                    Width = width,
                    Margin = new Thickness(lMargin, 0, 0, 0),
                };

                AddToGrid(breakdownOverlayGrid, breakdownGraphic, 0, 0);
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} left margin {lMargin.ToString("0.0")}";
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} right margin {rMargin.ToString("0.0")}";
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} width {width.ToString("0.0")}";
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} duration {(breakdown.BreakdownEnded - breakdown.BreakdownStarted).TotalMinutes.ToString("0.0")}";
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} started {breakdown.BreakdownStarted}";
                testTextBlock.Text += Environment.NewLine + $"breakdown #{i + 1} ended {breakdown.BreakdownEnded}";
            }

            double width2 = Math.Max((NewBreakdown.BreakdownEnded - NewBreakdown.BreakdownStarted).TotalMinutes * pxPerMinute, 0);
            double lMargin2 = (NewBreakdown.BreakdownStarted - Order.StartDate).TotalMinutes * pxPerMinute;

            DisplayBreakdown newBreakdownGraphic = new()
            {
                CodeText = NewBreakdown.BreakdownCode,
                Width = width2,
                Margin = new Thickness(lMargin2, 0, 0, 0),
            };

            newBreakdownGraphic.SetPending(true);

            AddToGrid(newBreakdownOverlayGrid, newBreakdownGraphic, 0, 0);

        }

        private static void AddToGrid(Grid grid, UIElement control, int column, int row, int colSpan = 1, int rowSpan = 1)
        {
            grid.Children.Add(control);
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            Grid.SetColumnSpan(control, colSpan);
            Grid.SetRowSpan(control, rowSpan);
        }

        private void BreakdownViewGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

    }
}
