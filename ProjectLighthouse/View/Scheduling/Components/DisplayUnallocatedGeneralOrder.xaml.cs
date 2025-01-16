using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.ValueConverters;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayUnallocatedGeneralOrder : UserControl
    {
        public GeneralManufactureOrder Order
        {
            get { return (GeneralManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }
        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(GeneralManufactureOrder), typeof(DisplayUnallocatedGeneralOrder), new PropertyMetadata(null, SetValues));


        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayUnallocatedGeneralOrder), new PropertyMetadata(null, SetIsEnabled));

        private static void SetIsEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedGeneralOrder control) return;
            control.MainButton.IsEnabled = control.SelectItemCommand is not null;
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedGeneralOrder control) return;
            if (control.Order is null) return;

            control.OrderText.Text = control.Order.Name;
            control.EstimatedRuntimeTextBlock.Text = $"{new intToTimespanString().Convert(control.Order.TimeToComplete, typeof(string), null, new CultureInfo("en-GB")) ?? "N/A"}";



            control.RequirementDueText.Text = $"{control.Order.RequiredDate:dd/MM/yy}";
        }

        public DisplayUnallocatedGeneralOrder()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.SelectItemCommand?.Execute(this.Order);
            this.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["PurpleLight"];
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["DisabledElement"];
        }

        private void MainButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not ToggleButton button) return;
            if (!button.IsChecked ?? false) return;

            if (App.CurrentUser.Role < UserRole.Scheduling)
            {
                return;
            }

            TimelineOrder newOrderControl = new()
            {
                MinDate = DateTime.MinValue,
                MaxDate = DateTime.MaxValue,
                Item = this.Order,
            };

            DragDrop.DoDragDrop(this,
                                 newOrderControl,
                                 DragDropEffects.Move);
        }
    }
}
