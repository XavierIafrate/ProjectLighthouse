using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.CheckSheets.Components
{
    public partial class ShowProductsList : UserControl
    {
        public CheckSheetsViewModel ViewModel
        {
            get { return (CheckSheetsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CheckSheetsViewModel), typeof(ShowProductsList), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ShowProductsList control)
            {
                return;
            }

            control.DataContext = control.ViewModel;
        }

        public ShowProductsList()
        {
            InitializeComponent();
        }
    }
}
