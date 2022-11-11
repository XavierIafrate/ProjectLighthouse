using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace View.Scheduling
{
    public partial class GanttView : Window
    {
        private List<ScheduleItem> items;
        public GanttView()
        {
            InitializeComponent();
        }

        public void MakeGantt(List<ScheduleItem> items)
        {
            mainGrid.RowDefinitions.Add(new RowDefinition());
            items = items.Where(x => x is not ScheduleWarning).ToList();
            this.items = items;
            DateTime minDate = items.Min(x => x.StartDate).Date;
            ScheduleItem LastStarting = items.OrderByDescending(x => x.StartDate).First();
            DateTime maxDate = LastStarting.StartDate.AddSeconds(LastStarting.TimeToComplete);
            TimeSpan viewWindow = maxDate - minDate;

            for (int i = 0; i < (int)viewWindow.TotalDays; i++)
            {
                mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

                bool even = i % 2 == 0;
                Border border = new() { Background = (Brush)Application.Current.Resources["Blue"] };
                mainGrid.Children.Add(border);

                TextBlock test = new()
                {
                    Text = minDate.AddDays(i).ToString("dd/MM"), 
                    VerticalAlignment = VerticalAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground= Brushes.White,
                };
                mainGrid.Children.Add(test);

                Grid.SetColumn(border, i);
                Grid.SetColumn(test, i);
            }

            for (int i = 0; i < items.Count; i++)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition());
                Border border = new()
                {
                    Background = (Brush)Application.Current.Resources["Blue"],
                    Height = 40,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new(2),
                };
                mainGrid.Children.Add(border);
                Grid.SetRow(border, i + 1);
                Grid.SetColumn(border, (int)(items[i].StartDate - minDate).TotalDays);
                Grid.SetColumnSpan(border, (int)TimeSpan.FromSeconds(items[i].TimeToComplete).TotalDays + 1);

                TextBlock test = new() 
                { 
                    Text = items[i].Name, 
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground= Brushes.White,
                    Margin= new(5),
                };
                mainGrid.Children.Add(test);
                Grid.SetColumn(test, (int)(items[i].StartDate - minDate).TotalDays);
                Grid.SetRow(test, i + 1);
                Grid.SetColumnSpan(test, (int)TimeSpan.FromSeconds(items[i].TimeToComplete).TotalDays + 1);
            }
        }
    }
}
