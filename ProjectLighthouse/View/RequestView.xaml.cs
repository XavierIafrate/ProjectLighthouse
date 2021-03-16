using ProjectLighthouse.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for RequestView.xaml
    /// </summary>
    public partial class RequestView : UserControl
    {
        RequestViewModel viewModel;
        public RequestView()
        {
            InitializeComponent();
            viewModel = Resources["vm"] as RequestViewModel;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdateRequest();
        }
    }
}
