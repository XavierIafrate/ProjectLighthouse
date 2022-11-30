using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace View.HelperWindows
{
    public partial class CalculationsHelperWindow : Window
    {
        public CalculationsHelperWindow()
        {
            InitializeComponent();

            LoadCostingData();
        }

        private void AllowNumsOnlyWithPeriod(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox)
            {
                return;
            }

            e.Handled = TextBoxHelper.ValidateKeyPressNumbersAndPeriod(textbox.Text, e);
        }

        private void AllowNumsOnlyNoPeriod(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox)
            {
                return;
            }

            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void AcrossCornersValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (double.TryParse(AcrossCornersValue.Text, out double ac))
            {
                double af = ac * Math.Cos((30 * Math.PI) / 180);
                AcrossFlatsValue.Text = $"{af:0.000000}";
                AcrossFlatsErrorText.Visibility = Visibility.Hidden;
                return;
            }

            AcrossFlatsErrorText.Text = "Failed to parse Across Corners value to double";
            AcrossFlatsErrorText.Visibility = Visibility.Hidden;
        }

        private void AcrossFlatsValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (double.TryParse(AcrossFlatsValue.Text, out double af))
            {
                double ac = af / Math.Cos((30 * Math.PI) / 180);
                AcrossCornersValue.Text = $"{ac:0.000000}";
                AcrossFlatsErrorText.Visibility = Visibility.Hidden;
                return;
            }

            AcrossFlatsErrorText.Text = "Failed to parse Across Flats value to double";
            AcrossFlatsErrorText.Visibility = Visibility.Hidden;
        }

        private void AcrossCornersValueCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AcrossCornersValue.Text);
        }
        private void AcrossFlatsValueCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AcrossFlatsValue.Text);
        }


        private List<BarStock> bars;
        private List<MaterialInfo> materials;

        // Inputs validated
        private int userQuantity = 0;
        //private double userDiameter = 0;
        private double userLength = 0;
        private int userBarLength = 0;
        private double userBarCost = 0;
        private int userCtMin = 2;
        private int userCtSec = 0;
        private int userToolingCost = 0;


        // Calculated
        public int partsPerBar = 0;
        public int numberOfBarsRequired = 0;
        public double totalBarCost = 0;
        public const double machineTimePerMin = 0.45;
        private TimeSpan totalTime;
        private double totalMachineCost = 0;
        public double totalCost = 0;


        private void LoadCostingData()
        {
            bars = DatabaseHelper.Read<BarStock>()
                .OrderBy(x => x.MaterialId)
                .ThenBy(x => x.Size)
                .ToList();

            materials = DatabaseHelper.Read<MaterialInfo>().ToList();

            bars.ForEach(b => b.MaterialData = materials.Find(x => x.Id == b.MaterialId));

            costingBarId.ItemsSource = bars;
            //TODO
            costingMaterialCombo.ItemsSource = bars.Select(x => x.MaterialData).Distinct().OrderBy(x => x).Prepend(null);
            costingMaterialCombo.SelectedIndex = 0;
        }

        private void CalculateCosts()
        {
            if (userLength > 0 && userBarLength > 300)
            {
                partsPerBar = (int)Math.Floor((userBarLength - 300) / userLength);
                costingPartsPerBar.Text = $"{partsPerBar:0} parts per bar.";
            }

            if (userQuantity > 0 && partsPerBar > 0)
            {
                numberOfBarsRequired = (int)Math.Ceiling((double)userQuantity / partsPerBar);
                costingNumBars.Text = $"# bar required: {numberOfBarsRequired:0}";
            }

            if (numberOfBarsRequired > 0 && userBarCost > 0)
            {
                totalBarCost = numberOfBarsRequired * userBarCost;
                costingTotalBarCost.Text = $"Cost of Bar: £{totalBarCost:#,##0.00}";
            }

            if ((userCtMin > 0 || userCtSec > 0) && userQuantity > 0)
            {

                totalTime = new(hours: 0, minutes: userCtMin, seconds: userCtMin);
                totalTime *= userQuantity;

                costingEstimatedTotalTime.Text = $"Appx. time to make: {totalTime.TotalDays:0.0} days";

                totalMachineCost = Math.Max(totalTime.TotalMinutes, 24 * 60) * machineTimePerMin;
                costingTotalMachineCost.Text = $"Total Machine Cost: £{totalMachineCost:#,##0.00}";
            }

            totalCost = totalMachineCost + totalBarCost + userToolingCost;
            costingTotalCost.Text = $"Total Cost: ~£{totalCost:#,##0.00}";

            if (totalCost > 0 && userQuantity > 0)
            {
                double unitCost = totalCost / userQuantity;
                costingUnitCost.Text = $"Unit Cost: £{unitCost:#,##0.00}";
            }
        }

        private void costingQuantity_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (int.TryParse(costingQuantity.Text, out int q))
            {
                if (q > 0)
                {
                    valid = true;
                    userQuantity = q;
                }
            }
            costingQuantity.BorderBrush = valid
                ? Brushes.Transparent
                : (Brush)Application.Current.Resources["Red"];
        }

        private void costingDiameter_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (double.TryParse(costingDiameter.Text, out double d))
            {
                if (d > 0) valid = true;
            }

            costingDiameter.BorderBrush = valid
                ? Brushes.Transparent
                : (Brush)Application.Current.Resources["Red"];
        }

        private void costingLength_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (double.TryParse(costingLength.Text, out double l))
            {
                if (l > 0)
                {
                    valid = true;
                    userLength = l;
                }
            }

            costingLength.BorderBrush = valid
                ? Brushes.Transparent
                : (Brush)Application.Current.Resources["Red"];
        }

        private void costingMaterialCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)costingMaterialCombo.SelectedValue == "-")
            {
                costingBarId.ItemsSource = bars;
            }
            else
            {
                //if (double.TryParse(costingDiameter.Text, out double diameter))
                //{
                //    costingBarId.ItemsSource = bars.Where(x => x.Material == (string)costingMaterialCombo.SelectedValue && x.Size >= diameter);
                //}
                //else
                //{
                //    costingBarId.ItemsSource = bars.Where(x => x.Material == (string)costingMaterialCombo.SelectedValue);
                //}
            }
        }

        private void costingBarId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (costingBarId.SelectedValue is not BarStock bar) return;

            userBarLength = bar.Length;
            userBarCost = bar.Cost / 100;

            costingBarLength.Text = bar.Length.ToString("0");
            costingBarCost.Text = (bar.Cost / 100).ToString("0.00");
        }

        private void costingBarLength_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (int.TryParse(costingBarLength.Text, out int d))
            {
                if (d > 300)
                {
                    valid = true;
                    userBarLength = d;
                }

                costingBarLength.BorderBrush = valid
                    ? Brushes.Transparent
                    : (Brush)Application.Current.Resources["Red"];
            }
        }

        private void costingCycleTimeMin_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (int.TryParse(costingCycleTimeMin.Text, out int d))
            {
                if (d >= 0)
                {
                    valid = true;
                    userCtMin = d;
                }

                costingCycleTimeMin.BorderBrush = valid
                    ? Brushes.Transparent
                    : (Brush)Application.Current.Resources["Red"];
            }
        }

        private void costingCycleTimeSec_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (int.TryParse(costingCycleTimeSec.Text, out int d))
            {
                if (d > 0)
                {
                    valid = true;
                    userCtSec = d;
                }

                costingCycleTimeSec.BorderBrush = valid
                    ? Brushes.Transparent
                    : (Brush)Application.Current.Resources["Red"];
            }
        }

        private void costingToolCost_KeyUp(object sender, KeyEventArgs e)
        {
            bool valid = false;

            if (int.TryParse(costingToolCost.Text, out int d))
            {
                if (d > 0)
                {
                    valid = true;
                    userToolingCost = d;
                }

                costingToolCost.BorderBrush = valid
                    ? Brushes.Transparent
                    : (Brush)Application.Current.Resources["Red"];
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CalculateCosts();
        }
    }
}
