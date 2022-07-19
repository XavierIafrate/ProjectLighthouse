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
    /// Interaction logic for EditProductWindow.xaml
    /// </summary>
    public partial class EditProductWindow : Window
    {
        public TurnedProduct Product { get; set; }
        public List<BarStock> BarStock { get; set; }
        public EditProductWindow(TurnedProduct product)
        {
            InitializeComponent();

            Product = product;
            BarStock = DatabaseHelper.Read<BarStock>();
            DataContext = this;
        }
    }
}
