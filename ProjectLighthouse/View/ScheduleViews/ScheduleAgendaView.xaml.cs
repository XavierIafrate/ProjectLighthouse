using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    public partial class ScheduleAgendaView : UserControl
    {
        public ScheduleAgendaView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainScheduleView.Items.Count > 0)
            {
                mainScheduleView.ScrollIntoView(mainScheduleView.Items[0]);
            }
        }
    }
}
