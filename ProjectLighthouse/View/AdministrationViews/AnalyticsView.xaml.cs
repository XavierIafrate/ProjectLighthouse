using System;
using bpac;
using System.Windows.Controls;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class AnalyticsView : UserControl
    {
        public Func<double, string> TimeFormatter = value => new DateTime((long)value).ToString("dd MMMM yyyy");
        public Func<double, string> QuantityFormatter = value => $"{value:#,##0}";
        //public Func<double, string> ThousandPoundFormatter = value => $"£{value:#,##0}";
        //public Func<double, string> SecondsToDaysFormatter { get; set; }
        //public Func<double, string> PercantageStringFormat = value => string.Format("{0}%", value);

        public AnalyticsView()
        {
            InitializeComponent();
            SetGraphFormatters();
        }

        private void SetGraphFormatters()
        {
            AllTimeParts_TimeAxis.LabelFormatter = TimeFormatter;
            AllTimeParts_CountAxis.LabelFormatter = QuantityFormatter;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string templatePath = @"C:\Users\x.iafrate\Desktop\nameplate1.lbx";


            Document doc = new();
			if (doc.Open(templatePath) != false)
			{

                bool test = doc.SetPrinter("Brother VC - 500W", true);
				doc.GetObject("objCompany").Text = "Hello";
				doc.GetObject("objName").Text = "World";

                doc.StartPrint("", bpac.PrintOptionConstants.bpoDefault);
                doc.PrintOut(1, bpac.PrintOptionConstants.bpoDefault);
                doc.EndPrint();
				doc.Close();
                MessageBox.Show("worked, maybe");
			}
			else
			{
				MessageBox.Show("Open() Error: " + doc.ErrorCode);
			}
		}
    }
}
