using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayResearch : UserControl
    {
        public ResearchTime Research
        {
            get { return (ResearchTime)GetValue(ResearchProperty); }
            set { SetValue(ResearchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Research.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResearchProperty =
            DependencyProperty.Register("Research", typeof(ResearchTime), typeof(DisplayResearch), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayResearch control)
            {
                return;
            }
        }

        public DisplayResearch()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetDateWindow dateWindow = new(Research as ScheduleItem);
            dateWindow.ShowDialog();

            if (dateWindow.SaveExit)
            {
                Research.StartDate = dateWindow.SelectedDate;
                Research.AllocatedMachine = dateWindow.AllocatedMachine;
                DatabaseHelper.Update(Research);
                Research.NotifyEditMade();
            }
        }
    }
}
