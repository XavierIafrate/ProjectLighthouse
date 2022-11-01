using Microsoft.Win32;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using View.Administration;
using ViewModel.Commands.Administration;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class LatheViewModel : BaseViewModel
    {
        public List<Lathe> Lathes { get; set; }
        private List<Lathe> filteredLathes;

        public List<Lathe> FilteredLathes
        {
            get { return filteredLathes; }
            set { filteredLathes = value; OnPropertyChanged(); }
        }

        private Lathe selectedLathe;

        public Lathe SelectedLathe
        {
            get { return selectedLathe; }
            set 
            { 
                selectedLathe = value; 
                OnPropertyChanged(); 
                LatheSelected = value != null; 
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
                searchTerm = value.ToUpper();
                OnPropertyChanged();
                FilterData();
            }
        }

        private Attachment selectedAttachment;

        public Attachment SelectedAttachment
        {
            get { return selectedAttachment; }
            set { selectedAttachment = value; OnPropertyChanged(); }
        }


        public MaintenanceEvent SelectedMaintenanceEvent { get; set; }

        public AddMaintenanceEventCommand AddMaintenanceEventCmd { get; set; }
        public EditMaintenanceCommand EditMaintenanceEventCmd { get; set; }
        public AddAttachmentToLatheCommand AddAttachmentCmd { get; set; }
        public RemoveAttachmentFromLatheCommand RemoveAttachmentCmd { get; set; }
        public AddLatheCommand AddLatheCmd { get; set; }
        public EditLatheCommand EditLatheCmd { get; set; }

        public List<Attachment> Attachments { get; set; }

        private bool latheSelected;
        public bool LatheSelected
        {
            get { return latheSelected; }
            set { latheSelected = value; OnPropertyChanged(); }
        }

        public LatheViewModel()
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
            Lathes = DatabaseHelper.Read<Lathe>().ToList();
            MaintenanceEvents = DatabaseHelper.Read<MaintenanceEvent>().ToList();
            Attachments = DatabaseHelper.Read<Attachment>().ToList();

            foreach (Lathe lathe in Lathes)
            {
                lathe.Maintenance = MaintenanceEvents
                    .Where(x => x.Lathe == lathe.Id)
                    .ToList();

                lathe.Attachments = Attachments.Where(x => x.DocumentReference == $"l{lathe.Id}").ToList();
                lathe.ServiceRecords = Attachments.Where(x => lathe.Maintenance.Any(y => $"s{y.Id}" == x.DocumentReference)).ToList();
            }
        }

        private void FilterData()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                FilteredLathes = new(Lathes);

            }
            else
            {
                FilteredLathes = Lathes.Where(x =>
                       x.IPAddress.Contains(SearchTerm)
                    || x.FullName.ToUpperInvariant().Contains(SearchTerm)
                    || x.Id.ToUpperInvariant().Contains(SearchTerm)
                    || x.Make.ToUpperInvariant().Contains(SearchTerm)
                    || x.Model.ToUpperInvariant().Contains(SearchTerm)
                    ).ToList();
            }

            if (FilteredLathes.Count > 0)
            {
                SelectedLathe = FilteredLathes.First();
            }
        }

        public void AddLathe()
        {
            AddLatheWindow window = new(Lathes) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();
        }

        public void EditLathe()
        {
            AddLatheWindow window = new(Lathes, SelectedLathe) { Owner = App.MainViewModel.MainWindow };
            window.ShowDialog();
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

            Attachment attachment = new($"l{SelectedLathe.Id}");
            attachment.CopyToStore(openFileDialog.FileName);
            DatabaseHelper.Insert(attachment);

            string currLathe = SelectedLathe.Id;
            GetData();
            FilterData();
            SelectedLathe = FilteredLathes.Find(x => x.Id == currLathe);
        }

        public void RemoveAttachment()
        {
            if (SelectedAttachment == null)
            {
                return;
            }

            if (MessageBox.Show($"Are you sure you want to remove {SelectedAttachment.FileName + SelectedAttachment.Extension}?{Environment.NewLine}This cannot be un-done.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            DatabaseHelper.Delete(SelectedAttachment);

            string currLathe = SelectedLathe.Id;
            GetData();
            FilterData();
            SelectedLathe = FilteredLathes.Find(x => x.Id == currLathe);
        }

        public void AddMaintenanceEvent()
        {
            CreateMaintenanceEventWindow window = new(SelectedLathe);
            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }
            MaintenanceEvents.Add(window.AddedEvent);
            string currLathe = SelectedLathe.Id;
            GetData();
            FilterData();
            SelectedLathe = FilteredLathes.Find(x => x.Id == currLathe);            
        }

        public void EditMaintenanceEvent()
        {
            CreateMaintenanceEventWindow window = new(SelectedLathe, SelectedMaintenanceEvent)
            {
                Owner = App.MainViewModel.MainWindow
            };

            window.ShowDialog();

            if (!window.SaveExit)
            {
                return;
            }

            string currLathe = SelectedLathe.Id;
            GetData();
            FilterData();
            SelectedLathe = FilteredLathes.Find(x => x.Id == currLathe);
        }
    }
}
