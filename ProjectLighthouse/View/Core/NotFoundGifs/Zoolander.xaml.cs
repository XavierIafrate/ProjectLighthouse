using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Core.NotFoundGifs
{
    public partial class Zoolander : UserControl
    {
        public string SearchString
        {
            get { return (string)GetValue(SearchStringProperty); }
            set { SetValue(SearchStringProperty, value); }
        }

        public static readonly DependencyProperty SearchStringProperty =
            DependencyProperty.Register("SearchString", typeof(string), typeof(Zoolander), new PropertyMetadata("center"));


        public Zoolander()
        {
            InitializeComponent();
        }
    }
}
