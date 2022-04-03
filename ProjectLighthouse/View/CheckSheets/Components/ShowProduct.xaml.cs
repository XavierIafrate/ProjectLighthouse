using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.CheckSheets.Components
{
    public partial class ShowProduct : UserControl
    {



        public CheckSheetsViewModel ViewModel
        {
            get { return (CheckSheetsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CheckSheetsViewModel), typeof(ShowProduct), new PropertyMetadata(null, SetValues));


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ShowProduct control)
            {
                return;
            }
            control.DataContext = control.ViewModel;
        }

        public ShowProduct()
        {
            InitializeComponent();
        }
    }
}
