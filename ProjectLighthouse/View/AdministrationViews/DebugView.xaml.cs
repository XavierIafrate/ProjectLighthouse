using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Reporting;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.IO;

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
            ReportPdf reportService = new();
            PerformanceReportData reportData = CreateReportData();

            string path = GetTempPdfPath();
            reportService.Export(path, reportData);

            reportService.OpenPdf(path);
        }

        private static PerformanceReportData CreateReportData()
        {
            return new PerformanceReportData
            {
                Patient = new Patient
                {
                    Id = "38561948",
                    FirstName = "Daniel",
                    LastName = "Price",
                    Birthdate = new DateTime(1970, 1, 1)
                },
                StructureSet = new StructureSet
                {
                    Id = "Test01",
                    Image = new Model.Reporting.Image
                    {
                        Id = "TestImageID",
                        CreationTime = new DateTime(1970, 1, 1, 12, 0, 0)
                    },
                    Structures = new[]
                    {
                        new DataStructure
                        {
                            Id = "Row0",
                            Efficiency = 90,
                            GoodPartsMade = 1000
                        },
                        new DataStructure
                        {
                            Id = "Row1",
                            Efficiency = 50,
                            GoodPartsMade = 100
                        },
                    }
                }
            };

        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }
    }
}
