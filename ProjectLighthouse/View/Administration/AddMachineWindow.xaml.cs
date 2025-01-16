using ProjectLighthouse.Model.Administration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Administration
{
    public partial class AddMachineWindow : Window, INotifyPropertyChanged
    {
        private Machine? originalMachine;

        private Machine machine;
        public Machine Machine
        {
            get { return machine; }
            set { machine = value; OnPropertyChanged(); }
        }

        private bool editMode;

        public bool EditMode
        {
            get { return editMode; }
            set { editMode = value; OnPropertyChanged(); }
        }


        public bool SaveExit { get; set; }
        private List<Machine> existingMachines;

        public AddMachineWindow(List<Machine> existingMachines, Machine? machine = null)
        {
            InitializeComponent();

            this.existingMachines = existingMachines;

            if (machine == null)
            {
                this.Title = "New Machine";
                EditMode = false;
                Machine = new();
                idTextBox.IsEnabled = true;
            }
            else
            {
                this.Title = "Edit Machine";
                EditMode = true;
                originalMachine = machine;
                Machine = machine.Clone();
                idTextBox.IsEnabled = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Machine.ValidateAll();
            if (Machine.HasErrors)
            {
                return;
            }

            if (existingMachines.Any(x => x.FullName == Machine.FullName && x.Id != Machine.Id))
            {
                MessageBox.Show($"Name '{Machine.FullName}' is already in use", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (originalMachine.IsUpdated(machine))
            {
                SaveExit = true;
            }

            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Machine.ValidateAll();
            if (Machine.HasErrors)
            {
                return;
            }

            if (existingMachines.Any(x => x.Id == Machine.Id))
            {
                MessageBox.Show($"ID '{Machine.Id}' is already in use", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (existingMachines.Any(x => x.FullName == Machine.FullName))
            {
                MessageBox.Show($"Name '{Machine.FullName}' is already in use", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveExit = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
