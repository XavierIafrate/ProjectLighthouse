using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for DebugView.xaml
    /// </summary>
    public partial class DebugView : UserControl
    {
        TextBoxOutputter outputter;

        public DebugView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);

            Console.WriteLine("Updating Users");

            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                user.LastLoginText = user.LastLogin.ToString("s");
                DatabaseHelper.Update(user);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<MachineOperatingBlock> stats = DatabaseHelper.Read<MachineOperatingBlock>();
            CSVHelper.WriteListToCSV(stats, "Performance");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }
    }
}
