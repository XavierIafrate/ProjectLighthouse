using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Commands.Administration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class LatheInspector : UserControl, INotifyPropertyChanged
    {
        public Lathe Lathe
        {
            get { return (Lathe)GetValue(LatheProperty); }
            set { SetValue(LatheProperty, value); }
        }

        public static readonly DependencyProperty LatheProperty =
            DependencyProperty.Register("Lathe", typeof(Lathe), typeof(LatheInspector), new PropertyMetadata(null));


        private Attachment selectedAttachment;
        public Attachment SelectedAttachment
        {
            get { return selectedAttachment; }
            set { selectedAttachment = value; OnPropertyChanged(); }
        }

        private MaintenanceEvent selectedMaintenanceEvent;
        public MaintenanceEvent SelectedMaintenanceEvent
        {
            get { return selectedMaintenanceEvent; }
            set { selectedMaintenanceEvent = value; OnPropertyChanged(); }
        }



        public AddMaintenanceEventCommand AddMaintenanceCommand
        {
            get { return (AddMaintenanceEventCommand)GetValue(AddMaintenanceCommandProperty); }
            set { SetValue(AddMaintenanceCommandProperty, value); }
        }

        public static readonly DependencyProperty AddMaintenanceCommandProperty =
            DependencyProperty.Register("AddMaintenanceCommand", typeof(AddMaintenanceEventCommand), typeof(LatheInspector), new PropertyMetadata(null));

        public EditMaintenanceCommand EditMaintenanceCommand
        {
            get { return (EditMaintenanceCommand)GetValue(EditMaintenanceCommandProperty); }
            set { SetValue(EditMaintenanceCommandProperty, value); }
        }

        public static readonly DependencyProperty EditMaintenanceCommandProperty =
            DependencyProperty.Register("EditMaintenanceCommand", typeof(EditMaintenanceCommand), typeof(LatheInspector), new PropertyMetadata(null));

        public AddAttachmentToLatheCommand AddAttachmentCommand
        {
            get { return (AddAttachmentToLatheCommand)GetValue(AddAttachmentCommandProperty); }
            set { SetValue(AddAttachmentCommandProperty, value); }
        }

        public static readonly DependencyProperty AddAttachmentCommandProperty =
            DependencyProperty.Register("AddAttachmentCommand", typeof(AddAttachmentToLatheCommand), typeof(LatheInspector), new PropertyMetadata(null));


        public RemoveAttachmentFromLatheCommand RemoveAttachmentCommand
        {
            get { return (RemoveAttachmentFromLatheCommand)GetValue(RemoveAttachmentCommandProperty); }
            set { SetValue(RemoveAttachmentCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveAttachmentCommandProperty =
            DependencyProperty.Register("RemoveAttachmentCommand", typeof(RemoveAttachmentFromLatheCommand), typeof(LatheInspector), new PropertyMetadata(null));




        private void MaintenanceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView lst) return;
            EditMaintenanceButton.IsEnabled = lst.SelectedValue is MaintenanceEvent;
        }

        private void AttachmentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView lst) return;
            RemoveAttachmentButton.IsEnabled = lst.SelectedValue is Attachment;
        }

        public LatheInspector()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
