using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls
{
    public partial class LMOQuantityDisplay : UserControl
    {
        public int RequiredQuantity
        {
            get { return (int)GetValue(RequiredQuantityProperty); }
            set { SetValue(RequiredQuantityProperty, value); }
        }

        public static readonly DependencyProperty RequiredQuantityProperty =
            DependencyProperty.Register("RequiredQuantity", typeof(int), typeof(LMOQuantityDisplay), new PropertyMetadata(0));

        public int MadeQuantity
        {
            get { return (int)GetValue(MadeQuantityProperty); }
            set { SetValue(MadeQuantityProperty, value); }
        }

        public static readonly DependencyProperty MadeQuantityProperty =
            DependencyProperty.Register("MadeQuantity", typeof(int), typeof(LMOQuantityDisplay), new PropertyMetadata(0));

        public int TargetQuantity
        {
            get { return (int)GetValue(TargetQuantityProperty); }
            set { SetValue(TargetQuantityProperty, value); }
        }

        public static readonly DependencyProperty TargetQuantityProperty =
            DependencyProperty.Register("TargetQuantity", typeof(int), typeof(LMOQuantityDisplay), new PropertyMetadata(0));


        public LMOQuantityDisplay()
        {
            InitializeComponent();
        }
    }
}
