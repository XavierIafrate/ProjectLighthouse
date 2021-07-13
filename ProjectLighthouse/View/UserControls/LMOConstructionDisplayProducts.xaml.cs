using ProjectLighthouse.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ProjectLighthouse.View.UserControls
{
    public partial class LMOConstructionDisplayProducts : UserControl
    {
        public TurnedProduct Product
        {
            get { return (TurnedProduct)GetValue(ProductProperty); }
            set { SetValue(ProductProperty, value); }
        }

        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register("Product", typeof(TurnedProduct), typeof(LMOConstructionDisplayProducts), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LMOConstructionDisplayProducts control)
                return;

            control.DataContext = control.Product;
            int recommendedQuantity = control.Product.GetRecommendedQuantity();

            control.recommendedQtyText.Text = String.Format("{0:#,##0} pcs", recommendedQuantity);

            if (recommendedQuantity >= 1200)
            {
                control.RatingImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/MO-3star.png"));
            }
            else if (recommendedQuantity >= 500)
            {
                control.RatingImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/MO-2star.png"));
            }
            else
            {
                control.RatingImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/MO-1star.png"));
            }
        }

        public LMOConstructionDisplayProducts()
        {
            InitializeComponent();
        }
    }
}
