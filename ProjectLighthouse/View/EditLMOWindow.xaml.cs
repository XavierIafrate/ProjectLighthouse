using ProjectLighthouse.Model;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EditLMOWindow.xaml
    /// </summary>
    public partial class EditLMOWindow : Window
    {
        private LatheManufactureOrder order;
        private ObservableCollection<LatheManufactureOrderItem> items;
        
        public EditLMOWindow(LatheManufactureOrder o)
        {
            InitializeComponent();
            order = o;
            items = new ObservableCollection<LatheManufactureOrderItem>();
            PopulateControls();
        }

        private void PopulateControls()
        {
            var itemsList = DatabaseHelper.Read<LatheManufactureOrderItem>().Where(n => n.AssignedMO == order.Name).ToList();
            items.Clear();
            foreach (var product in itemsList)
            {
                items.Add(product);
            }
            ItemsListBox.ItemsSource = items;

            nameText.Text = order.Name;
            PORef.Text = order.POReference;
            notes.AppendText(order.Notes);
            notes.Document.LineHeight = 2;
            //urgent.IsChecked = order.IsUrgent;
            program.IsChecked = order.HasProgram;
            ready.IsChecked = order.IsReady;
            

            if (!order.HasProgram)
            {
                ready.IsEnabled = false;
            }

            if (!App.currentUser.CanEditLMOs)
            {
                PORef.IsEnabled = false;
                setters.IsEnabled = false;
            }

            var users = DatabaseHelper.Read<User>().Where(n => n.UserRole == "Production").ToList();
            List<string> setterUsers = new List<string>();
            
            foreach (var user in users)
            {
                setterUsers.Add(user.GetFullName());
            }
            setters.ItemsSource = setterUsers.ToList();
            setters.Text = order.AllocatedSetter;

            
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(notes.Document.ContentStart, notes.Document.ContentEnd);
            order.Notes = textRange.Text.Substring(0, textRange.Text.Length - 1);
            order.POReference = PORef.Text;
            order.AllocatedSetter = setters.Text;
            //order.IsUrgent = urgent.IsChecked ?? false;
            order.HasProgram = program.IsChecked ?? false;
            order.IsReady = ready.IsChecked ?? false;

            if (order.IsReady)
            {
                order.Status = "Ready";
            }
            else
            {
                order.Status = "Problem";
            }

            order.ModifiedBy = App.currentUser.GetFullName();
            order.ModifiedAt = DateTime.Now;

            DatabaseHelper.Update(order);
            
            this.Close();
        }

        private void program_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked??false)
            {
                ready.IsEnabled = true;
            }
            else
            {
                ready.IsEnabled = false;
                ready.IsChecked = false;
            }
        }

        private void DisplayLMOItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisplayLMOItems control = sender as DisplayLMOItems;
            LatheManufactureOrderItem item = control.LatheManufactureOrderItem;
            EditLMOItemWindow editWindow = new EditLMOItemWindow(item);
            editWindow.ShowDialog();
            PopulateControls();
            //RecalculateTime();
        }
    }
}
