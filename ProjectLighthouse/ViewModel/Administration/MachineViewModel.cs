using Microsoft.Win32;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class MachineViewModel : BaseViewModel
    {
        private List<Machine> machines;


        private List<Machine> filteredMachines;
        public List<Machine> FilteredMachines
        {
            get { return filteredMachines; }
            set { filteredMachines = value; OnPropertyChanged(); }
        }


        private Machine selectedMachine;
        public Machine SelectedMachine
        {
            get { return selectedMachine; }
            set
            {
                selectedMachine = value;
                OnPropertyChanged();
            }
        }

        private List<MaintenanceEvent> maintenanceEvents;
        public List<MaintenanceEvent> MaintenanceEvents
        {
            get { return maintenanceEvents; }
            set
            {
                maintenanceEvents = value;
                OnPropertyChanged();
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value;
                OnPropertyChanged();
                FilterData();
            }
        }

        public AddMaintenanceEventCommand AddMaintenanceEventCmd { get; set; }
        public EditMaintenanceCommand EditMaintenanceEventCmd { get; set; }

        public AddAttachmentToLatheCommand AddAttachmentCmd { get; set; }
        public RemoveAttachmentFromLatheCommand RemoveAttachmentCmd { get; set; }

        public AddMachineCommand AddLatheCmd { get; set; }
        public EditMachineCommand EditLatheCmd { get; set; }

        public List<Attachment> Attachments { get; set; }


        public MachineViewModel()
        {
            AddMaintenanceEventCmd = new(this);
            EditMaintenanceEventCmd = new(this);

            AddAttachmentCmd = new(this);
            RemoveAttachmentCmd = new(this);

            AddLatheCmd = new(this);
            EditLatheCmd = new(this);

            GetData();
            FilterData();
        }

        private void GetData()
        {
            machines = new();
            machines.AddRange(DatabaseHelper.Read<Lathe>().ToList());
            machines.AddRange(DatabaseHelper.Read<Machine>().ToList());
            machines = machines.OrderBy(x => x.Id).ToList();
            MaintenanceEvents = DatabaseHelper.Read<MaintenanceEvent>().ToList();
            Attachments = DatabaseHelper.Read<Attachment>().ToList();

            foreach (Machine machine in machines)
            {
                if (machine is Lathe lathe)
                {
                    lathe.Maintenance = MaintenanceEvents
                        .Where(x => x.Lathe == lathe.Id)
                        .ToList();

                    lathe.Attachments = Attachments.Where(x => x.DocumentReference == $"l{machine.Id}").ToList();
                    lathe.ServiceRecords = Attachments.Where(x => lathe.Maintenance.Any(y => $"s{y.Id}" == x.DocumentReference)).ToList();
                    lathe.ValidateAll();
                }
            }
        }

        private void FilterData()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredMachines = new(machines);

            }
            else
            {
                string sanitisedSearchString = SearchTerm.ToUpperInvariant().Trim();
                List<Machine> results = new();
                foreach (Machine machine in machines)
                {
                    if (
                           machine.FullName.Contains(sanitisedSearchString, StringComparison.InvariantCultureIgnoreCase)
                        || machine.Id.Contains(sanitisedSearchString, StringComparison.InvariantCultureIgnoreCase)
                        || machine.Make.Contains(sanitisedSearchString, StringComparison.InvariantCultureIgnoreCase)
                        || machine.Model.Contains(sanitisedSearchString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        results.Add(machine);
                        continue;
                    }

                    if (machine is Lathe lathe)
                    {
                        if (lathe.IPAddress.Contains(sanitisedSearchString, StringComparison.InvariantCultureIgnoreCase))
                        {
                            results.Add(machine);
                        }
                    }
                }

                FilteredMachines = results;
            }

            if (FilteredMachines.Count > 0)
            {
                SelectedMachine = FilteredMachines.First();
            }
        }

        public void AddMachine()
        {
            AddMachineWindow window = new(machines) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            window.Machine.CreatedAt = DateTime.Now;
            window.Machine.CreatedBy = App.CurrentUser.UserName;

            try
            {
                DatabaseHelper.Insert<Machine>(window.Machine, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error adding to DB", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GetData();
            FilterData();

            SelectedMachine = FilteredMachines.Find(x => x.FullName == window.Machine.FullName);
        }

        public void EditMachine(Machine machine)
        {
            AddMachineWindow window = new(machines, machine) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            try
            {
                DatabaseHelper.Update<Machine>(window.Machine, throwErrs: true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error updating DB", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            GetData();
            FilterData();

            SelectedMachine = FilteredMachines.Find(x => x.Id == window.Machine.Id);
        }

        public void AddLathe()
        {
            AddLatheWindow window = new(machines) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            GetData();
            FilterData();

            SelectedMachine = FilteredMachines.Find(x => x.Id == window.Lathe.Id);
        }

        public void EditLathe(Lathe lathe)
        {
            AddLatheWindow window = new(machines, lathe) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            GetData();
            FilterData();

            SelectedMachine = FilteredMachines.Find(x => x.Id == window.Lathe.Id);
        }

        public void AddAttachment()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "PDF documents|*.pdf|Text files (*.txt)|*.txt|Excel Workbooks|*.xlsx|Images|*.png;*.jpg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == false) return;
            if (!File.Exists(openFileDialog.FileName))
            {
                return;
            }

            Attachment attachment = new($"l{SelectedMachine.Id}");
            attachment.CopyToStore(openFileDialog.FileName);
            DatabaseHelper.Insert(attachment);

            string currMachine = SelectedMachine.Id;
            GetData();
            FilterData();
            SelectedMachine = FilteredMachines.Find(x => x.Id == currMachine);
        }

        public void RemoveAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                return;
            }

            if (MessageBox.Show($"Are you sure you want to remove {attachment.FileName + attachment.Extension}?{Environment.NewLine}This cannot be un-done.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!DatabaseHelper.Delete(attachment))
            {
                MessageBox.Show("Failed to delete from database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string currentMachine = SelectedMachine.Id;
            GetData();
            FilterData();
            SelectedMachine = FilteredMachines.Find(x => x.Id == currentMachine);
        }

        public void AddMaintenanceEvent()
        {
            if (SelectedMachine is not Lathe lathe) return;
            CreateMaintenanceEventWindow window = new(lathe) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            MaintenanceEvents.Add(window.AddedEvent);
            string currentMachine = SelectedMachine.Id;
            GetData();
            FilterData();
            SelectedMachine = FilteredMachines.Find(x => x.Id == currentMachine);
        }

        public void EditMaintenanceEvent(MaintenanceEvent maintenanceEvent)
        {
            if (SelectedMachine is not Lathe lathe) return;
            CreateMaintenanceEventWindow window = new(lathe, maintenanceEvent)
            {
                Owner = App.MainViewModel.MainWindow
            };

            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            string currentMachine = SelectedMachine.Id;
            GetData();
            FilterData();
            SelectedMachine = FilteredMachines.Find(x => x.Id == currentMachine);
        }
    }
}
