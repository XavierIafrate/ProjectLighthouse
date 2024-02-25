using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
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
    public partial class DisplayUnallocatedService : UserControl
    {
        public MachineService Service
        {
            get { return (MachineService)GetValue(ServiceProperty); }
            set { SetValue(ServiceProperty, value); }
        }

        public static readonly DependencyProperty ServiceProperty =
            DependencyProperty.Register("Service", typeof(MachineService), typeof(DisplayUnallocatedService), new PropertyMetadata(null, SetValues));

        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayUnallocatedService), new PropertyMetadata(null, SetIsEnabled));

        private static void SetIsEnabled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedService control) return;
            control.MainButton.IsEnabled = control.SelectItemCommand is not null;
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedService control) return;
            if (control.Service is null) return;

            control.OrderText.Text = control.Service.Name;
            control.EstimatedRuntimeTextBlock.Text = $"{new intToTimespanString().Convert(control.Service.TimeToComplete, typeof(string), null, new CultureInfo("en-GB")) ?? "N/A"}";
        }

        public DisplayUnallocatedService()
        {
            InitializeComponent();
            this.MainButton.IsEnabled = false;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.SelectItemCommand?.Execute(this.Service);
            this.BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["OrangeLight"];
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
                Item = this.Service,
            };

            DragDrop.DoDragDrop(this,
                                 newOrderControl,
                                 DragDropEffects.Move);
        }
    }
}
