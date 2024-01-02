using ProjectLighthouse.Model.Quality;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Converters;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayToleranceDefinition : UserControl
    {
        public ToleranceDefinition Tolerance
        {
            get { return (ToleranceDefinition)GetValue(ToleranceProperty); }
            set { SetValue(ToleranceProperty, value); }
        }

        public static readonly DependencyProperty ToleranceProperty =
            DependencyProperty.Register("Tolerance", typeof(ToleranceDefinition), typeof(DisplayToleranceDefinition), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayToleranceDefinition control) return;
            if (control.Tolerance is null)
            {
                control.AddressText.Text = "Reference not found";
                return;
            }

            ToleranceDefinition tol = control.Tolerance;

            control.NominalText.Text = tol.Nominal;
            control.DescriptionText.Text = tol.Name;
            control.AddressText.Text = tol.Id;

            switch (tol.ToleranceType)
            {
                case ToleranceType.None:
                    control.MinText.Visibility = Visibility.Collapsed;
                    control.MaxText.Visibility = Visibility.Collapsed;

                    break;
                case ToleranceType.Min:
                    control.MinText.Visibility = Visibility.Collapsed;
                    control.MaxText.Text = " MIN";
                    control.NominalText.Text = tol.LowerLimit;
                    break;
                case ToleranceType.Max:
                    control.MinText.Visibility = Visibility.Collapsed;
                    control.MaxText.Text = " MAX";
                    control.NominalText.Text = tol.UpperLimit;
                    break;
                case ToleranceType.Symmetric:
                    control.MinText.Visibility = Visibility.Collapsed;
                    control.MaxText.Text = " ± " + tol.Max.ToString(tol.StringFormatter);
                    break;
                case ToleranceType.Bilateral:
                    control.MinText.Text = (tol.Min * -1).ToString($" +{tol.StringFormatter}; -{tol.StringFormatter}; -{tol.StringFormatter}");
                    control.MaxText.Text = tol.Max.ToString($" +{tol.StringFormatter}; -{tol.StringFormatter}; +{tol.StringFormatter}");
                    break;
                case ToleranceType.Fit:
                    control.MinText.Visibility = Visibility.Collapsed;
                    control.MaxText.Text = " " + tol.FitId;
                    break;
                default:

                    break;
            };

        }

        public DisplayToleranceDefinition()
        {
            InitializeComponent();
        }

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ToleranceDefinition)) is ToleranceDefinition draggedDef)
            {
                if (draggedDef.Id == this.Tolerance.Id)
                {
                    return;
                }
            }

            Point position = e.GetPosition(this);
            double height = this.ActualHeight;

            if (position.Y < height / 2)
            {
                bottomBorder.Visibility = Visibility.Collapsed;
                topBorder.Visibility = Visibility.Visible;
            }
            else
            {

                bottomBorder.Visibility = Visibility.Visible;
                topBorder.Visibility = Visibility.Collapsed;
            }


        }

        private void UserControl_DragLeave(object sender, DragEventArgs e)
        {
            bottomBorder.Visibility= Visibility.Collapsed;
            topBorder.Visibility= Visibility.Collapsed;
            
            if(e.Data.GetData(typeof(ToleranceDefinition)) is ToleranceDefinition draggedDef) 
            { 
                if (draggedDef.Id == this.Tolerance.Id)
                {
                    this.Opacity = 0.5;
                }
            }
        }

        public void RemoveDragFormats()
        {
            this.Opacity = 1;
            bottomBorder.Visibility = Visibility.Collapsed;
            topBorder.Visibility = Visibility.Collapsed;
        }
    }
}
