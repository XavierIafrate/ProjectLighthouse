using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
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

namespace ProjectLighthouse.View.HelperWindows
{
    /// <summary>
    /// Interaction logic for RaiseSpecialRequest.xaml
    /// </summary>
    public partial class RaiseSpecialRequest : Window
    {
        public TurnedProduct NewProduct { get; set; }
        public bool productAdded = false;
        public RaiseSpecialRequest()
        {
            InitializeComponent();
            NewProduct = new();
        }

        public void Submit()
        {
            _ = DatabaseHelper.Insert<TurnedProduct>(NewProduct);
            productAdded = true;
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"drawing: {NewProduct.SpecificationDocument}");
        }
    }
}
