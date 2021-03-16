using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayRequestCard.xaml
    /// </summary>
    public partial class DisplayRequestCard : UserControl
    {
        public Request Request
        {
            get { return (Request)GetValue(RequestProperty); }
            set { SetValue(RequestProperty, value); }
        }

                // Using a DependencyProperty as the backing store for Request.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RequestProperty =
            DependencyProperty.Register("Request", typeof(Request), typeof(DisplayRequestCard), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DisplayRequestCard displayRequestCard = d as DisplayRequestCard;
            //if (displayRequestCard.Request)
            //{
            //    displayRequestCard.DataContext = displayRequestCard.Request;

            //    Debug.WriteLine(displayRequestCard.Request.Product);

            //    //if (!String.IsNullOrEmpty(displayRequestCard.Request.ModifiedBy)){
            //    //    displayRequestCard.ModifiedByTextBlock.Visibility = Visibility.Visible;
            //    //    displayRequestCard.ModifiedDateTextBox.Visibility = Visibility.Visible;
            //    //}
            //    //else
            //    //{
            //    //    displayRequestCard.ModifiedByTextBlock.Visibility = Visibility.Collapsed;
            //    //    displayRequestCard.ModifiedDateTextBox.Visibility = Visibility.Collapsed;
            //    //}
            //}
        }

        private void updateRequest()
        {
            if(Request != null)
            {
                Request.LastModified = DateTime.Now;
                Request.ModifiedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
                
                if (DatabaseHelper.Update(Request))
                {
                    MessageBox.Show("Request updated with exit code 0 (Card updateRequest)", "Success");
                }
            }
            else
            {
                MessageBox.Show("Request is empty");
            }
        }

        public DisplayRequestCard()
        {
            InitializeComponent();
            Request = new Request();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            updateRequest();
        }
    }
}
