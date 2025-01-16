using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DraggableNewService : UserControl
    {
        private MachineService machineService = new();
        bool allowDrag;
        public DraggableNewService()
        {
            InitializeComponent();

            if (App.CurrentUser.Role < UserRole.Scheduling)
            {
                DecrementDaysButton.IsEnabled = false;
                IncrementDaysButton.IsEnabled = false;
                DecrementHoursButton.IsEnabled = false;
                IncrementHoursButton.IsEnabled = false;
                ServiceNameTextbox.IsEnabled = false;

                TipText.Text = "insufficient permissions";
            }
        }

        private void DecrementHoursButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            machineService.TimeToComplete -= 3600;
            SetTimeText();
            SetControlColours();
        }

        private void SetTimeText()
        {
            HoursIndicatorTextBlock.Text = $"{(machineService.TimeToComplete % 86400) / 3600}h";
            DaysIndicatorTextBlock.Text = $"{(machineService.TimeToComplete - machineService.TimeToComplete % 86400) / 86400}d";
            DecrementDaysButton.IsEnabled = machineService.TimeToComplete >= 86400;
            DecrementHoursButton.IsEnabled = machineService.TimeToComplete >= 3600;
        }

        private void IncrementDaysButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            machineService.TimeToComplete += 86400;
            SetTimeText();
            SetControlColours();
        }

        private void DecrementDaysButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            machineService.TimeToComplete -= 86400;
            SetTimeText();
            SetControlColours();
        }

        private void IncrementHoursButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            machineService.TimeToComplete += 3600;
            SetTimeText();
            SetControlColours();
        }

        private void ServiceNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            machineService.Name = ServiceNameTextbox.Text.Trim();
            SetControlColours();
        }

        void SetControlColours()
        {
            if (machineService.TimeToComplete > 0 && !string.IsNullOrWhiteSpace(machineService.Name))
            {
                BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["Orange"];
                BackgroundBorder.Background = (Brush)Application.Current.Resources["OrangeFaded"];

                ServiceNameTextbox.Foreground = (Brush)Application.Current.Resources["Orange"];
                RequiredNameLabel.Foreground = (Brush)Application.Current.Resources["Orange"];
                DaysIndicatorTextBlock.Foreground = (Brush)Application.Current.Resources["Orange"];
                HoursIndicatorTextBlock.Foreground = (Brush)Application.Current.Resources["Orange"];

                TipText.Text = "drag to schedule";
                TipText.Foreground = (Brush)Application.Current.Resources["Orange"];

                this.AllowDrop = true;
                this.allowDrag = true;
            }
            else
            {
                BackgroundBorder.BorderBrush = (Brush)Application.Current.Resources["OnBackground"];
                BackgroundBorder.Background = (Brush)Application.Current.Resources["Surface"];

                ServiceNameTextbox.Foreground = (Brush)Application.Current.Resources["OnBackground"];
                RequiredNameLabel.Foreground = (Brush)Application.Current.Resources["OnBackground"];
                DaysIndicatorTextBlock.Foreground = (Brush)Application.Current.Resources["OnBackground"];
                HoursIndicatorTextBlock.Foreground = (Brush)Application.Current.Resources["OnBackground"];

                TipText.Text = "enter data to enable drag";
                TipText.Foreground = (Brush)Application.Current.Resources["OnBackground"];

                this.AllowDrop = false;
                this.allowDrag = false;
            }
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (App.CurrentUser.Role < UserRole.Scheduling)
            {
                return;
            }

            if (allowDrag)
            {
                if (this.machineService.Clone() is not MachineService newService)
                {
                    MessageBox.Show("Could not clone service");
                    return;
                }

                newService.Id = 0;

                TimelineOrder newServiceControl = new()
                {
                    MinDate = DateTime.MinValue,
                    MaxDate = DateTime.MaxValue,
                    Item = newService,
                };

                DragDrop.DoDragDrop(this,
                                     newServiceControl,
                                     DragDropEffects.Move);
            }
        }
    }
}
