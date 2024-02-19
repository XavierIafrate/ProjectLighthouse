using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Windows.Graphics.Display;
using static ProjectLighthouse.Model.Scheduling.ProductionSchedule;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayUnallocatedItems : UserControl
    {
        public List<ScheduleItem> UnallocatedItems
        {
            get { return (List<ScheduleItem>)GetValue(UnallocatedItemsProperty); }
            set { SetValue(UnallocatedItemsProperty, value); }
        }

        public static readonly DependencyProperty UnallocatedItemsProperty =
            DependencyProperty.Register("UnallocatedItems", typeof(List<ScheduleItem>), typeof(DisplayUnallocatedItems), new PropertyMetadata(null, SetValues));


        public RescheduleItemCommand RescheduleCommand
        {
            get { return (RescheduleItemCommand)GetValue(RescheduleCommandProperty); }
            set { SetValue(RescheduleCommandProperty, value); }
        }

        public static readonly DependencyProperty RescheduleCommandProperty =
            DependencyProperty.Register("RescheduleCommand", typeof(RescheduleItemCommand), typeof(DisplayUnallocatedItems), new PropertyMetadata(null));


        public ScheduleItem? SelectedItem
        {
            get { return (ScheduleItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ScheduleItem), typeof(DisplayUnallocatedItems), new PropertyMetadata(null, SetSelectedItems));

        private static void SetSelectedItems(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedItems control) return;

            foreach (DisplayUnallocatedOrder itemControl in FindVisualChildren<DisplayUnallocatedOrder>(control.MainItemsControl))
            {
                if (control.SelectedItem == null)
                {
                    itemControl.MainButton.IsChecked = false; 
                    continue;
                }
                itemControl.MainButton.IsChecked = itemControl.Order.Name == control.SelectedItem.Name;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is not null and T t)
                {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }

        }

        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayUnallocatedItems), new PropertyMetadata(null));



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayUnallocatedItems control) return;
            if (control.UnallocatedItems == null) return;

            if (control.UnallocatedItems.Count == 0)
            {
                control.NoItemsMessage.Visibility = Visibility.Visible;
                control.MainItemsControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                control.NoItemsMessage.Visibility = Visibility.Collapsed;
                control.MainItemsControl.Visibility = Visibility.Visible;
                if (control.MainItemsControl.ItemsSource != null)
                {
                    control.MainItemsControl.Items.Refresh();
                }
                else
                {
                    control.MainItemsControl.ItemsSource = control.UnallocatedItems;
                }
            }
        }


        public DisplayUnallocatedItems()
        {
            InitializeComponent();
        }


        private void ToggleButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
        }

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {

        }

        private void UserControl_DragLeave(object sender, DragEventArgs e)
        {
            this.DropZone.Visibility = Visibility.Hidden;
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            this.DropZone.Visibility = Visibility.Visible;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            this.DropZone.Visibility = Visibility.Hidden;

        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if(sender is not ToggleButton button) return;
            if (button.Content is not ScheduleItem item) return;

            if (this.SelectItemCommand is null)
            {
                MessageBox.Show("Command is null");
                return;
            }

            if (this.SelectedItem == item) return;

            this.SelectItemCommand?.Execute(item);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton button) return;
            if (button.Content is not ScheduleItem item) return;

            if (this.SelectedItem is null) return;
            if (this.SelectedItem == item)
            {
                this.SelectItemCommand?.Execute(null);
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (RescheduleCommand is null) return;
            if (e.Data.GetData(typeof(TimelineOrder)) is not TimelineOrder orderControl) return;
            this.DropZone.Visibility = Visibility.Collapsed;


            ScheduleItem item = orderControl.Item;

            if (string.IsNullOrEmpty(item.AllocatedMachine))
            {
                return;
            }

            RescheduleInformation rescheduleParams = new(item, null, null);

            RescheduleCommand?.Execute(rescheduleParams);
        }
    }
}
