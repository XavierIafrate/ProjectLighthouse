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

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for CheckSheetEditor.xaml
    /// </summary>
    public partial class CheckSheetEditor : Window
    {
        public CheckSheetEditor()
        {
            InitializeComponent();
            pdfViewer.Navigate(new Uri("file:///C:/Users/x.iafrate/Downloads/529-38602.pdf"));
        }
    }
}
