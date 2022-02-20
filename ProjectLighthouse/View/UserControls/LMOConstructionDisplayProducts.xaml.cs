using ProjectLighthouse.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            control.recommendedQtyText.Text = $"{recommendedQuantity:#,##0} pcs";
            Geometry g;
            Brush b;

            if (recommendedQuantity >= 1200)
            {
                g = Geometry.Parse("M17.75,9L18.95,8.24C19.58,8.58 20,9.24 20,10V21.75C20,21.75 12,20 12,11V10C12,9.27 12.39,8.63 12.97,8.28L14.43,9L16,8L17.75,9M14,2C15.53,2 16.8,3.15 17,4.64C18,4.93 18.81,5.67 19.22,6.63L17.75,7.5L16,6.5L14.43,7.5L12.76,6.67C13.15,5.72 13.95,5 14.94,4.66C14.8,4.28 14.43,4 14,4V2M10,10C10,18 13.63,19.84 16,21.75C16,21.75 8,20 8,11V10C8,9.27 8.39,8.63 8.97,8.28L10.3,8.94C10.11,9.25 10,9.61 10,10M10.43,7.5L8.76,6.67C9.15,5.72 9.95,5 10.94,4.66C10.8,4.28 10.43,4 10,4V2C10.77,2 11.47,2.29 12,2.76V4C12.43,4 12.8,4.28 12.94,4.66C11.95,5 11.15,5.72 10.43,7.5M6,10C6,18 9.63,19.84 12,21.75C12,21.75 4,20 4,11V10C4,9.27 4.39,8.63 4.97,8.28L6.3,8.94C6.11,9.25 6,9.61 6,10M6.43,7.5L4.76,6.67C5.15,5.72 5.95,5 6.94,4.66C6.8,4.28 6.43,4 6,4V2C6.77,2 7.47,2.29 8,2.76V4C8.43,4 8.8,4.28 8.94,4.66C7.95,5 7.15,5.72 6.43,7.5Z");
                b = (Brush)Application.Current.Resources["materialError"];
            }
            else if (recommendedQuantity >= 500)
            {
                g = Geometry.Parse("M15.75, 9L16.95, 8.24C17.58, 8.58 18, 9.24 18, 10V21.75C18, 21.75 10, 20 10, 11V10C10, 9.27 10.39, 8.63 10.97, 8.28L12.43, 9L14, 8L15.75, 9M12, 2C13.53, 2 14.8, 3.15 15, 4.64C16, 4.93 16.81, 5.67 17.22, 6.63L15.75, 7.5L14, 6.5L12.43, 7.5L10.76, 6.67C11.15, 5.72 11.95, 5 12.94, 4.66C12.8, 4.28 12.43, 4 12, 4V2M8, 10C8, 18 11.63, 19.84 14, 21.75C14, 21.75 6, 20 6, 11V10C6, 9.27 6.39, 8.63 6.97, 8.28L8.3, 8.94C8.11, 9.25 8, 9.61 8, 10M8.43, 7.5L6.76, 6.67C7.15, 5.72 7.95, 5 8.94, 4.66C8.8, 4.28 8.43, 4 8, 4V2C8.77, 2 9.47, 2.29 10, 2.76V4C10.43, 4 10.8, 4.28 10.94, 4.66C9.95, 5 9.15, 5.72 8.43, 7.5Z");
                b = Brushes.Orange;
            }
            else
            {
                g = Geometry.Parse("M13.75,9L14.95,8.24C15.58,8.58 16,9.24 16,10V21.75C16,21.75 8,20 8,11V10C8,9.27 8.39,8.63 8.97,8.28L10.43,9L12,8L13.75,9M10,2C11.53,2 12.8,3.15 13,4.64C14,4.93 14.81,5.67 15.22,6.63L13.75,7.5L12,6.5L10.43,7.5L8.76,6.67C9.15,5.72 9.95,5 10.94,4.66C10.8,4.28 10.43,4 10,4V2Z");
                b = (Brush)Application.Current.Resources["materialPrimaryGreen"];
            }

            control.RatingImage.Data = g;
            control.RatingImage.Fill = b;
        }

        public LMOConstructionDisplayProducts()
        {
            InitializeComponent();
        }
    }
}
