using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class ScheduleItemInspector : UserControl, INotifyPropertyChanged
    {
        public Dictionary<User, int> OrderAssignmentCounts
        {
            get { return (Dictionary<User, int>)GetValue(OrderAssignmentCountsProperty); }
            set { SetValue(OrderAssignmentCountsProperty, value); }
        }

        public static readonly DependencyProperty OrderAssignmentCountsProperty =
             DependencyProperty.Register("OrderAssignmentCounts", typeof(Dictionary<User, int>), typeof(ScheduleItemInspector), new PropertyMetadata(new Dictionary<User, int>()));


        public ScheduleItem Item
        {
            get { return (ScheduleItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ScheduleItem), typeof(ScheduleItemInspector), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;
            control.MainScrollViewer.ScrollToHome();
        }

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(ScheduleItemInspector), new PropertyMetadata(false, SetEditMode));

        private static void SetEditMode(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScheduleItemInspector control) return;

            if (control.EditMode)
            {
                control.ContentGrid.ColumnDefinitions[0].Width = new(100);
            }
            else
            {
                control.ContentGrid.ColumnDefinitions[0].Width = new(1, GridUnitType.Star);
            }
        }

        public OrderViewModelRelayCommand RelayCommand
        {
            get { return (OrderViewModelRelayCommand)GetValue(RelayCommandProperty); }
            set { SetValue(RelayCommandProperty, value); }
        }

        public static readonly DependencyProperty RelayCommandProperty =
            DependencyProperty.Register("RelayCommand", typeof(OrderViewModelRelayCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null));


        public SendMessageCommand SendMessageCommand
        {
            get { return (SendMessageCommand)GetValue(SendMessageCommandProperty); }
            set { SetValue(SendMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty SendMessageCommandProperty =
            DependencyProperty.Register("SendMessageCommand", typeof(SendMessageCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null));



        public DeleteNoteCommand DeleteMessageCommand
        {
            get { return (DeleteNoteCommand)GetValue(DeleteMessageCommandProperty); }
            set { SetValue(DeleteMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMessageCommandProperty =
            DependencyProperty.Register("DeleteMessageCommand", typeof(DeleteNoteCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null));

        public SaveNoteCommand SaveMessageCommand
        {
            get { return (SaveNoteCommand)GetValue(SaveMessageCommandProperty); }
            set { SetValue(SaveMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty SaveMessageCommandProperty =
            DependencyProperty.Register("SaveMessageCommand", typeof(SaveNoteCommand), typeof(ScheduleItemInspector), new PropertyMetadata(null));



        private List<User> productionStaff;

        public List<User> ProductionStaff
        {
            get { return productionStaff; }
            set { productionStaff = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ScheduleItemInspector()
        {
            InitializeComponent();
            List<User> users = DatabaseHelper.Read<User>();
            ProductionStaff = users.Where(x => x.Role == UserRole.Production).OrderBy(x => x.UserName).Append(new() { FirstName = "Unassigned", UserName = null }).ToList();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            gradTop.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;

            gradBottom.Visibility = scrollViewer.VerticalOffset + 1 > scrollViewer.ScrollableHeight
               ? Visibility.Hidden
               : Visibility.Visible;
        }
    }
}
