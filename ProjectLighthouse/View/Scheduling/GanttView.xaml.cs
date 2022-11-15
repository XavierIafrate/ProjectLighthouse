using Model.Scheduling;
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
        private GanttData data;
        public GanttView()
        {
            InitializeComponent();
        }

        public void Draw(GanttData ganttData)
        {
            this.data = ganttData;

            DateTime minDate = ganttData.Data.Min(x => x.Events.Min(y => y.StartDate));
            DateTime maxDate = ganttData.Data.Max(x => x.Events.Max(y => y.EndsAt()));

            DrawGrid(minDate, maxDate, ganttData.Data);
            DrawHeader(minDate, maxDate);
            DrawDivisions(ganttData.Data, minDate);

            //DateTime maxDate = LastStarting.StartDate.AddSeconds(LastStarting.TimeToComplete);

            //for (int i = 0; i < (int)viewWindow.TotalDays; i++)
            //{
            //    mainGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //    if (i % 7 != 0)
            //    {
            //        continue;
            //    }

            //    Border border = new() { Background = (Brush)Application.Current.Resources["Blue"] };
            //    mainGrid.Children.Add(border);

            //    TextBlock headerText = new()
            //    {
            //        Text = minDate.AddDays(i).ToString("dd/MM"), 
            //        VerticalAlignment = VerticalAlignment.Center, 
            //        HorizontalAlignment = HorizontalAlignment.Center,
            //        Foreground= Brushes.White,
            //    };

            //    mainGrid.Children.Add(headerText);

            //    Grid.SetColumn(headerText, i);
            //    Grid.SetColumn(border, i);

            //    Grid.SetColumnSpan(headerText, 7);
            //    Grid.SetColumnSpan(border, 7);
            //}

            //for (int i = 0; i < items.Count; i++)
            //{
            //    Border border = new()
            //    {
            //        Background = (Brush)Application.Current.Resources["Blue"],
            //        Height = 40,
            //        BorderBrush = Brushes.Black,
            //        BorderThickness = new(2),
            //    };
            //    mainGrid.Children.Add(border);
            //    Grid.SetRow(border, i + 1);
            //    Grid.SetColumn(border, (int)(items[i].StartDate - minDate).TotalDays);
            //    Grid.SetColumnSpan(border, (int)TimeSpan.FromSeconds(items[i].TimeToComplete).TotalDays + 1);

            //    TextBlock test = new() 
            //    { 
            //        Text = items[i].Name, 
            //        VerticalAlignment = VerticalAlignment.Center,
            //        Foreground= Brushes.White,
            //        Margin= new(5),
            //    };
            //    mainGrid.Children.Add(test);
            //    Grid.SetColumn(test, (int)(items[i].StartDate - minDate).TotalDays);
            //    Grid.SetRow(test, i + 1);
            //    Grid.SetColumnSpan(test, (int)TimeSpan.FromSeconds(items[i].TimeToComplete).TotalDays + 1);
            //}
        }

        private void DrawGrid(DateTime startDate, DateTime endDate, List<GanttDivision> divs)
        {
            // Row and Column Headers
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 150 });
            int numDataRows = 0;

            // Rows
            for (int i = 0; i < divs.Count; i++)
            {
                mainGrid.RowDefinitions.Add(new RowDefinition());
                numDataRows++;
                for (int j = 0; j < divs[i].Events.Count; j++)
                {
                    mainGrid.RowDefinitions.Add(new RowDefinition());
                    numDataRows++;
                }
            }

            // Columns
            TimeSpan span = endDate.Date - startDate.Date;
            int daysToCover = (int)span.TotalDays;
            for (int i = 0; i < daysToCover; i++)
            {
                DayOfWeek day = startDate.AddDays(i).DayOfWeek;
                if (day is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    for (int j = 0; j < numDataRows; j++)
                    {
                        Border border = new()
                        {
                            Background = (Brush)Application.Current.Resources["DisabledElement"],
                        };

                        AddToMainGrid(border, i + 1, j + 1);
                    }
                }

                mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void DrawHeader(DateTime startDate, DateTime endDate)
        {
            TimeSpan span = endDate.Date - startDate.Date;
            int daysToCover = (int)span.TotalDays;
            for (int i = 0; i < daysToCover; i++)
            {
                DateTime date = startDate.AddDays(i);
                DayOfWeek day = date.DayOfWeek;
                if (day is not DayOfWeek.Monday)
                {
                    continue;
                }

                TextBlock header = new()
                {
                    Text = date.ToString("dd-MMM"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                AddToMainGrid(header, i + 1, 0, 7);
            }
        }

        private void DrawDivisions(List<GanttDivision> divs, DateTime startDate)
        {
            int currRow = 1;
            for(int i = 0; i < divs.Count;i++)
            {
                DrawDivision(divs[i], currRow, startDate);

                currRow++;
                currRow += divs[i].Events.Count;
            }
        }

        private void DrawDivision(GanttDivision div, int startingRow, DateTime startDate)
        {
            TextBlock divHeader = new()
            {
                Text = div.Title
            };
            AddToMainGrid(divHeader, 0, startingRow);

            for(int i = 0; i < div.Events.Count;i++)
            {

                ScheduleItem e = div.Events[i]; 
                TextBlock eventHeader = new()
                {
                    Text = e.Name,
                };

                AddToMainGrid(eventHeader, 0, startingRow+i+1);

                Border ganttElement = new()
                {
                    Background = (Brush)Application.Current.Resources["Blue"],
                };

                int colStart = (int)(e.StartDate.Date - startDate).TotalDays+1; //??

                TimeSpan eventDuration = TimeSpan.FromSeconds(e.TimeToComplete);
                AddToMainGrid(ganttElement, 1 + colStart, startingRow + i + 1, (int)Math.Ceiling(eventDuration.TotalDays));

            }
        }


        private void AddToMainGrid(UIElement control, int column, int row, int colSpan = 1, int rowSpan = 1)
        {
            mainGrid.Children.Add(control);
            Grid.SetColumn(control, column);
            Grid.SetRow(control, row);
            Grid.SetColumnSpan(control, colSpan);
            Grid.SetRowSpan(control, rowSpan);
        }
    }
}
