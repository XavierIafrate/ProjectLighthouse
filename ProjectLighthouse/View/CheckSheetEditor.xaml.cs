using ABI.System;
using Model.Quality.Internal;
using LiveCharts.Wpf.Converters;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Quality;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectLighthouse.View
{
    public partial class CheckSheetEditor : Window
    {
        public List<CheckSheetDimension> Dimensions { get; set; }
        string PdfPath = @"C:\Users\x.iafrate\Desktop\Spooktoberfest_Invite_2022_v2[5751].pdf";
        public CheckSheetEditor()
        {
            InitializeComponent();
            
            Console.SetOut(new ControlWriter(ConsoleOutput));

            Dimensions = new()
            {
                new()
                {
                    Id = 1,
                    DrawingId = 5,
                    Name = "Head Chamfer",
                    IsNumeric = true,
                    NumericValue = 0.2,
                    ToleranceType = Model.Quality.ToleranceType.Basic,
                    StringFormatter = "0.00"
                },
                new()
                {
                    Id = 2,
                    DrawingId = 5,
                    Name = "Finish",
                    IsNumeric = false,
                    StringValue = "Clean",
                },
                new()
                {
                    Id = 3,
                    DrawingId = 5,
                    Name = "Thread Length",
                    IsNumeric = true,
                    NumericValue = 10.5,
                    ToleranceType = Model.Quality.ToleranceType.Bilateral,
                    Min = 0.5,
                    Max = 0,
                }
            };

            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(new(), Dimensions, null);


            foreach (CheckSheetDimension d in Dimensions)
            {
                Console.WriteLine($"{d.Name}");
                if (d.IsNumeric)
                {
                    Console.WriteLine($"Nominal: {d.NumericValue.ToString(d.StringFormatter)}");
                    Console.WriteLine($"Lower Limit: {d.LowerLimit}");
                    Console.WriteLine($"Upper Limit: {d.UpperLimit}");
                }
                else
                {
                    Console.WriteLine($"Nominal: {d.StringValue}");
                }
                Console.WriteLine("");
            }

            //int newId = DatabaseHelper.InsertAndReturnId(Dimensions.First());

            //Console.WriteLine($"Inserted dimension with ID: {newId}");

            DimensionGrid.ItemsSource = Dimensions;

        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Count of Dimensions: {Dimensions.Count}");
            DimensionCheckSheet checkSheet = new();
            checkSheet.BuildContent(new(), Dimensions, null);
            
        }

        public class ControlWriter : TextWriter
        {
            private TextBox textbox;
            public ControlWriter(TextBox textbox)
            {
                this.textbox = textbox;
            }

            public override void Write(char value)
            {
                textbox.Text += value;
            }

            public override void Write(string value)
            {
                textbox.Text += value;
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            //pdfViewer.Navigate(new Uri());
            //Browser.Address = "";
        }

        private void ToggleNavButton_Click(object sender, RoutedEventArgs e)
        {
            PdfPath = @"C:\Users\xavie\Documents\checksheet.pdf";


            System.Uri uri = new System.Uri(PdfPath);
            webView.Source = uri;

            webView.Reload();
        }
    }
}
