using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayBarOverview.xaml
    /// </summary>
    public partial class DisplayBarOverview : UserControl
    {
        public BarStockRequirementOverview Bar
        {
            get { return (BarStockRequirementOverview)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarProperty =
            DependencyProperty.Register("Bar", typeof(BarStockRequirementOverview), typeof(DisplayBarOverview), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayBarOverview control)
            {
                return;
            }

            

            control.BarID.Text = control.Bar.BarStock.Id;
            control.InStockText.Text = $"{control.Bar.BarStock.InStock} in Kasto"; // (appx. {mass:#,##0kg})
            control.OnOrderText.Text = $"{control.Bar.BarStock.OnOrder} on order";

            if (control.Bar.Orders.Count > 0)
            {
                control.NoneText.Visibility = Visibility.Collapsed;
                control.OrdersListBox.Visibility = Visibility.Visible;
                control.OrdersListBox.ItemsSource = control.Bar.Orders;
                control.ExtraInfo.Visibility = Visibility.Visible;
            }
            else
            {
                control.NoneText.Visibility = Visibility.Visible;
                control.OrdersListBox.Visibility = Visibility.Collapsed;
                control.ExtraInfo.Visibility = Visibility.Collapsed;
            }

            if (control.Bar.FreeBar >= 0)
            {
                control.SuggestionText.Visibility = Visibility.Collapsed;
                control.StatusBackground.Background = (Brush)Application.Current.Resources["materialPrimaryGreen"];
                control.StatusText.Text = $"Stock level OK. {control.Bar.FreeBar} free bars.";
                if (control.Bar.BarStock.InStock < control.Bar.BarsRequiredForOrders)
                {
                    control.WaitingFlag.Visibility = Visibility.Visible;
                    control.OKFlag.Visibility = Visibility.Collapsed;
                }
                else
                {
                    control.WaitingFlag.Visibility = Visibility.Collapsed;
                    control.OKFlag.Visibility = Visibility.Visible;
                }
                control.WarningFlag.Visibility = Visibility.Collapsed;

            }
            else
            {
                control.StatusBackground.Background = (Brush)Application.Current.Resources["materialError"];
                control.StatusText.Text = $"Stock level warning. {control.Bar.FreeBar} free bars.";
                if (control.Bar.BarStock.SuggestedStock > 0)
                {
                    control.SuggestionText.Text = $"Suggested Stock: {control.Bar.BarStock.SuggestedStock} bar(s)";
                    control.SuggestionText.Visibility = Visibility.Visible;
                }
                else
                {
                    control.SuggestionText.Visibility = Visibility.Collapsed;
                }
                control.WarningFlag.Visibility = Visibility.Visible;
                control.WaitingFlag.Visibility = Visibility.Collapsed;
                control.OKFlag.Visibility = Visibility.Collapsed;
            }

            control.RequiredBarsText.Text = $"{control.Bar.BarsRequiredForOrders} required for orders";

            //control.BarID.Text = control.Bar.BarStock.Id;
        }

        public DisplayBarOverview()
        {
            InitializeComponent();
        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Bar.BarStock.Id);
        }
    }
}
