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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View
{
    /// <summary>
    /// Interaction logic for DebugView.xaml
    /// </summary>
    public partial class DebugView : UserControl
    {
        TextBoxOutputter outputter;

        public DebugView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);

            Console.WriteLine("Updating Users");

            List<User> users = DatabaseHelper.Read<User>();

            foreach (User user in users)
            {
                user.LastLoginText = user.LastLogin.ToString("s");
                DatabaseHelper.Update(user);
            }
        }
    }
}
