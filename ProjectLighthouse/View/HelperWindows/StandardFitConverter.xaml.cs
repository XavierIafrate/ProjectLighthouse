using ProjectLighthouse;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Quality;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ProjectLighthouse.View.HelperWindows
{
    public partial class StandardFitConverter : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double basic;
        public double Basic
        {
            get { return basic; }
            set
            {
                if (value == basic) return; // allow decimal pt
                basic = value;
                OnPropertyChanged();
                SetTolerance();
            }
        }

        public StandardFitConverter()
        {
            InitializeComponent();

            if (App.StandardFits == null)
            {
                string fitsJson = File.ReadAllText($"{App.ROOT_PATH}fits.txt");
                App.StandardFits = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StandardFit>>(fitsJson);
            }

            this.fitsOptions.ItemsSource = App.StandardFits;
            SetTextToDefault();
        }

        void SetTolerance()
        {
            string stringFormatter = ToleranceDefinition.DecimalPlacesToStringFormatter(GetDecimalPlaces(Convert.ToDecimal(Basic)), relative:true);

            this.NominalText.Text = Basic.ToString(stringFormatter);


            if (this.fitsOptions.SelectedValue is not StandardFit selectedFit)
            {
                SetTextToDefault();
                return;
            }

            ToleranceZone? zone = selectedFit.At(Basic);

            if (zone == null)
            {
                SetTextToDefault();
                return;
            }

            double? min = zone.Min;
            double? max = zone.Max;

            if (min == null || max == null)
            {
                SetTextToDefault();
                return;
            }

            int numDecimalPlaces = GetDecimalPlaces(Convert.ToDecimal(min));
            numDecimalPlaces = Math.Max(numDecimalPlaces, GetDecimalPlaces(Convert.ToDecimal(max)));

            stringFormatter = ToleranceDefinition.DecimalPlacesToStringFormatter(numDecimalPlaces, relative: true);
            
            this.minText.Text = ((double)min).ToString(stringFormatter);
            this.maxText.Text = ((double)max).ToString(stringFormatter);
            this.NominalText.Text = Basic.ToString(stringFormatter);
            
        }

        void SetTextToDefault()
        {
            this.minText.Text = "-";
            this.maxText.Text = "-";
            this.NominalText.Text = "-";
        }

        private void fitsOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTolerance();
        }

        static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n); //make sure it is positive.
            n -= (int)n;     //remove the integer part of the number.
            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }
            return decimalPlaces;
        }
    }
}
