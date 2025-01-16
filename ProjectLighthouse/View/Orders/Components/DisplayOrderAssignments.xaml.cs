using ProjectLighthouse.Model.Administration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class DisplayOrderAssignments : UserControl, INotifyPropertyChanged
    {
        public Dictionary<User, int> Data
        {
            get { return (Dictionary<User, int>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(Dictionary<User, int>), typeof(DisplayOrderAssignments), new PropertyMetadata(new Dictionary<User, int>(), SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOrderAssignments control) return;
            control.Compute();
        }

        private void Compute()
        {
            if ((Data ?? new()).Count == 0)
            {
                FormattedData = new();
                return;
            }

            List<AssignmentDisplayData> newData = new();

            int max = Data!.Max(x => x.Value);

            foreach (KeyValuePair<User, int> item in Data!)
            {
                newData.Add(new()
                {
                    User = item.Key,
                    MaxCount = max,
                    AssignedCount = item.Value,
                });
            }
            newData = newData.OrderByDescending(x => x.User.UserName != "unassigned" ? 1 : 0).ThenByDescending(x => x.AssignedCount).ToList();
            TotalOrders = newData.Sum(x => x.AssignedCount);
            FormattedData = newData;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<AssignmentDisplayData> formattedData;

        public List<AssignmentDisplayData> FormattedData
        {
            get { return formattedData; }
            set { formattedData = value; OnPropertyChanged(); }
        }

        private int totalOrders;

        public int TotalOrders
        {
            get { return totalOrders; }
            set { totalOrders = value; OnPropertyChanged(); }
        }



        public DisplayOrderAssignments()
        {
            InitializeComponent();
        }

        public struct AssignmentDisplayData
        {
            public User User { get; set; }
            public int AssignedCount { get; set; }
            public int MaxCount { get; set; }
        }
    }
}
