using ProjectLighthouse.Model;
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

namespace ProjectLighthouse.View.AssemblyViews
{
    /// <summary>
    /// Interaction logic for EditAssemblyOrderWindow.xaml
    /// </summary>
    public partial class EditAssemblyOrderWindow : Window
    {
        private AssemblyManufactureOrder order;
        public AssemblyManufactureOrder Order
        {
            get { return order; }
            set 
            { 
                order = value;
                
                //orderTitle.Text = value.Name;
            }
        }

        private List<Drop> drops;

        public List<Drop> Drops
        {
            get { return drops; }
            set 
            { 
                drops = value;
                //dropsListBox.ItemsSource = value;
            }
        }


        public EditAssemblyOrderWindow()
        {
            InitializeComponent();
        }
    }
}
