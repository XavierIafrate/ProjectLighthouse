using ProjectLighthouse.Model.Orders;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Orders
{
    public partial class WorkloadWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<string, List<LatheManufactureOrder>> workload;
        public Dictionary<string, List<LatheManufactureOrder>> Workload
        {
            get { return workload; }
            set { workload = value; OnPropertyChanged(); }
        }

        public WorkloadWindow()
        {
            InitializeComponent();
            Workload = new();
        }

        public void SetWorkload(List<LatheManufactureOrder> orders)
        {
            Workload.Clear();

            orders = orders
                .Where(x => x.State < OrderState.Complete)
                .OrderBy(x => x.StartDate)
                .ToList();

            string[] assignedUsers = orders
                .Select(x => x.AssignedTo)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            for (int i = 0; i < assignedUsers.Length; i++)
            {
                List<LatheManufactureOrder> ordersForUser = orders.Where(x => x.AssignedTo == assignedUsers[i]).ToList();
                workload.Add(assignedUsers[i] ?? "Unassigned", ordersForUser);
            }

            OnPropertyChanged(nameof(Workload));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
