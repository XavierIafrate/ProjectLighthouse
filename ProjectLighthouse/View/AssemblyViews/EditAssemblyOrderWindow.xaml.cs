using ProjectLighthouse.Model;
using System.Collections.Generic;
using System.Windows;

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
