using ProjectLighthouse.Model.Orders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayOrderStateProgress : UserControl
    {
        public OrderState OrderState
        {
            get { return (OrderState)GetValue(OrderStateProperty); }
            set { SetValue(OrderStateProperty, value); }
        }

        public static readonly DependencyProperty OrderStateProperty =
            DependencyProperty.Register("OrderState", typeof(OrderState), typeof(DisplayOrderStateProgress), new PropertyMetadata(OrderState.Problem, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOrderStateProgress control) return;
            OrderState state = control.OrderState;
            Brush surfaceBrush = (Brush)Application.Current.Resources["Surface"];
            Brush redBrush = (Brush)Application.Current.Resources["RedFaded"];
            Brush greenBrush = (Brush)Application.Current.Resources["GreenFaded"];
            Brush tealBrush = (Brush)Application.Current.Resources["TealFaded"];
            Brush blueBrush = (Brush)Application.Current.Resources["BlueFaded"];
            Brush blackBrush = (Brush)Application.Current.Resources["BlackFaded"];

            control.ProblemCheck.Visibility = Visibility.Hidden;
            control.ReadyCheck.Visibility = Visibility.Hidden;
            control.PreparedCheck.Visibility = Visibility.Hidden;
            control.RunningCheck.Visibility = Visibility.Hidden;
            control.CompleteCheck.Visibility = Visibility.Hidden;

            control.readyBorder.Background = surfaceBrush;
            control.preparedBorder.Background = surfaceBrush;
            control.runningBorder.Background = surfaceBrush;
            control.completeBorder.Background = surfaceBrush;



            if (state > OrderState.Problem)
            {
                control.ProblemCheck.Visibility = Visibility.Visible;
                control.readyBorder.Background = greenBrush;
            }

            if (state > OrderState.Ready)
            {
                control.ReadyCheck.Visibility = Visibility.Visible;
                control.preparedBorder.Background = tealBrush;
            }

            if (state > OrderState.Prepared)
            {
                control.PreparedCheck.Visibility = Visibility.Visible;
                control.runningBorder.Background = blueBrush;
            }

            if (state > OrderState.Running)
            {
                control.RunningCheck.Visibility = Visibility.Visible;
                control.completeBorder.Background = blackBrush;
            }

        }

        public DisplayOrderStateProgress()
        {
            InitializeComponent();
        }
    }
}
