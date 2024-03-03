using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class PreFlightIndicators : UserControl, INotifyPropertyChanged
    {
        public LatheManufactureOrder Order
        {
            get { return (LatheManufactureOrder)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(LatheManufactureOrder), typeof(PreFlightIndicators), new PropertyMetadata(null, SetValues));

        private Brush programBrush = Brushes.Black;
        public Brush ProgramBrush
        {
            get { return programBrush; }
            set { programBrush = value; OnPropertyChanged(); }
        }

        private Brush toolingBrush = Brushes.Black;
        public Brush ToolingBrush
        {
            get { return toolingBrush; }
            set { toolingBrush = value; OnPropertyChanged(); }
        }

        private Brush barVerifiedBrush = Brushes.Black;
        public Brush BarVerifiedBrush
        {
            get { return barVerifiedBrush; }
            set { barVerifiedBrush = value; OnPropertyChanged(); }
        }

        private Brush barPreparedBrush = Brushes.Black;
        public Brush BarPreparedBrush
        {
            get { return barPreparedBrush; }
            set { barPreparedBrush = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged; 
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PreFlightIndicators control) return;
            if (e.OldValue is LatheManufactureOrder order)
            {
                order.PropertyChanged -= control.SetBrushes;
            }
            if (e.NewValue == null) return;
            control.Order.PropertyChanged += control.SetBrushes;
            control.ComputeBrushes();
        }

        private void SetBrushes(object sender, PropertyChangedEventArgs e)
        {
            string[] watchProps = new string[] 
            { 
                nameof(LatheManufactureOrder.HasProgram),
                nameof(LatheManufactureOrder.BaseProgramExists),
                nameof(LatheManufactureOrder.AllToolingReady),
                nameof(LatheManufactureOrder.ToolingOrdered),
                nameof(LatheManufactureOrder.ToolingReady),
                nameof(LatheManufactureOrder.BarToolingOrdered),
                nameof(LatheManufactureOrder.BarToolingReady),
                nameof(LatheManufactureOrder.GaugingOrdered),
                nameof(LatheManufactureOrder.GaugingReady),
                nameof(LatheManufactureOrder.BarIsVerified),
                nameof(LatheManufactureOrder.BarIsAllocated),
                nameof(LatheManufactureOrder.IsComplete),
                nameof(LatheManufactureOrder.IsCancelled),
            
            };

            if (watchProps.Contains(e.PropertyName))
            {
                ComputeBrushes();
            }
        }

        private void ComputeBrushes()
        {
            if (Order is null)
            {
                Brush blackBrush = (Brush)Application.Current.Resources["Black"];

                ProgramBrush = blackBrush;
                ToolingBrush = blackBrush;
                BarVerifiedBrush = blackBrush;
                BarPreparedBrush = blackBrush;
                return;
            }

            if (Order.State == OrderState.Complete)
            {
                Brush greenFadedBrush = (Brush)Application.Current.Resources["GreenFaded"];
                ProgramBrush = greenFadedBrush;
                ToolingBrush = greenFadedBrush;
                BarVerifiedBrush = greenFadedBrush;
                BarPreparedBrush = greenFadedBrush;
                return;
            }

            if (Order.State == OrderState.Cancelled)
            {
                Brush blackFadedBrush = (Brush)Application.Current.Resources["BlackFaded"];
                ProgramBrush = blackFadedBrush;
                ToolingBrush = blackFadedBrush;
                BarVerifiedBrush = blackFadedBrush;
                BarPreparedBrush = blackFadedBrush;
                return;
            }


            Brush redBrush = (Brush)Application.Current.Resources["Red"];
            Brush orangeBrush = (Brush)Application.Current.Resources["Orange"];
            Brush greenBrush = (Brush)Application.Current.Resources["Green"];

            // Program Brush setting
            if (Order.HasProgram)
            {
                ProgramBrush = greenBrush;
            }
            else if (Order.BaseProgramExists)
            {
                ProgramBrush = orangeBrush;
            }
            else
            {
                ProgramBrush = redBrush;
            }


            // Tooling Brush setting
            if (Order.AllToolingReady)
            {
                ToolingBrush = greenBrush;
            }
            else if ((Order.ToolingOrdered || Order.ToolingReady)
                    && (Order.BarToolingOrdered || Order.BarToolingReady)
                    && (Order.GaugingOrdered || Order.GaugingReady))
            {
                ToolingBrush = orangeBrush;
            }
            else
            {
                ToolingBrush = redBrush;
            }


            // Bar Verified Brush setting
            BarVerifiedBrush = Order.BarIsVerified ? greenBrush : redBrush;
            
            
            // Bar Prepared Brush setting
            if (Order.NumberOfBarsIssued >= Order.NumberOfBars)
            {
                BarPreparedBrush = greenBrush;
            }
            else if (Order.NumberOfBarsIssued > 0)
            {
                BarPreparedBrush = orangeBrush;
            }
            else
            {
                BarPreparedBrush = redBrush;
            }

        }

        public PreFlightIndicators()
        {
            InitializeComponent();
        }
    }
}
