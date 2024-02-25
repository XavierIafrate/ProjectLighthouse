using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.Scheduling.Components;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling
{
    public partial class ScheduleView : UserControl
    {
        public ScheduleView()
        {
            InitializeComponent();
        }

        private void TabControl_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is not TabControl tabControl) return;
            if (e.Data.GetData(typeof(TimelineOrder)) is not TimelineOrder orderControl) return;

            if (orderControl.Item is MachineService service)
            {
                if(service.Id == 0)
                {
                    return;
                }
            }

            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                TabItem x = (TabItem)tabControl.Items[i];
                if (x.IsEnabled && x.Header.ToString() == "Unallocated" )
                {
                    tabControl.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
