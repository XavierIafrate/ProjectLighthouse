using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


        //TODO: Add filters



namespace ProjectLighthouse.ViewModel
{
    public class RequestViewModel : BaseViewModel
    {
        public ObservableCollection<Request> Requests { get; set; }

        private Request selectedRequest;
        public Request SelectedRequest
        {
            get { return selectedRequest; }
            set 
            { 
                selectedRequest = value;
                OnPropertyChanged("SelectedRequest");
                SelectedRequestChanged?.Invoke(this, new EventArgs());

                if(selectedRequest == null)
                {
                    return;
                }

                if (String.IsNullOrEmpty(selectedRequest.ModifiedBy))
                {
                    ModifiedVis = Visibility.Collapsed;
                }
                else
                {
                    ModifiedVis = Visibility.Visible;
                }

                if(App.currentUser.CanApproveRequests && selectedRequest.Status == "Pending approval")
                {
                    ApprovalControlsVis = Visibility.Visible;
                }
                else
                {
                    ApprovalControlsVis = Visibility.Collapsed;
                }

                if(selectedRequest.Status == "Pending approval" && (App.currentUser.UserRole == "Production" || App.currentUser.UserRole == "admin"))
                {
                    ProductionCheckboxEnabled = true;
                }
                else
                {
                    ProductionCheckboxEnabled = false;
                }

                if (selectedRequest.Status == "Pending approval" && (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin"))
                {
                    SchedulingCheckboxEnabled = true;
                }
                else
                {
                    SchedulingCheckboxEnabled = false;
                }

                if (selectedRequest.Status == "Pending approval" && (App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin" || App.currentUser.UserRole == "Production"))
                {
                    DropboxEnabled = true;
                }
                else
                {
                    DropboxEnabled = false;
                }
            }
        }

        private bool productionCheckboxEnabled;
        public bool ProductionCheckboxEnabled
        {
            get { return productionCheckboxEnabled; }
            set 
            { 
                productionCheckboxEnabled = value;
                OnPropertyChanged("ProductionCheckboxEnabled");
            }
        }

        private bool dropboxEnabled;
        public bool DropboxEnabled
        {
            get { return dropboxEnabled; }
            set 
            { 
                dropboxEnabled = value;
                OnPropertyChanged("DropboxEnabled");
            }
        }


        private bool schedulingCheckboxEnabled;
        public bool SchedulingCheckboxEnabled
        {
            get { return schedulingCheckboxEnabled; }
            set 
            { 
                schedulingCheckboxEnabled = value;
                OnPropertyChanged("SchedulingCheckboxEnabled");
            }
        }


        private Visibility approvalControlsVis;
        public Visibility ApprovalControlsVis
        {
            get { return approvalControlsVis; }
            set 
            { 
                approvalControlsVis = value;
                OnPropertyChanged("ApprovalControlsVis");
            }
        }

        private Visibility modifiedVis;
        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set 
            { 
                modifiedVis = value;
                OnPropertyChanged("ModifiedVis");
            }
        }

        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set 
            { 
                selectedFilter = value; 
            }
        }


        public event EventHandler SelectedRequestChanged;

        public ApproveRequestCommand ApproveCommand { get; set; }
        public DeclineRequestCommand DeclineCommand { get; set; }

        public RequestViewModel()
        {
            Requests = new ObservableCollection<Request>();
            ApproveCommand = new ApproveRequestCommand(this);
            DeclineCommand = new DeclineRequestCommand(this);
            SelectedRequest = new Request();

            if (App.currentUser.CanApproveRequests)
            {
                approvalControlsVis = Visibility.Visible;
            }
            else
            {
                approvalControlsVis = Visibility.Collapsed;
            }

            GetRequests();
            if (Requests.Count > 0) 
            { 
                SelectedRequest = Requests.First(); 
            }
            
        }

        public void UpdateRequest()
        {
            SelectedRequest.LastModified = DateTime.Now;
            SelectedRequest.ModifiedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
            if (DatabaseHelper.Update(SelectedRequest))
            {
                OnPropertyChanged("SelectedRequest");
                ModifiedVis = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Failed to update", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ApproveRequest()
        {
            if(selectedRequest.isProductionApproved && selectedRequest.isSchedulingApproved && App.currentUser.CanApproveRequests)
            {
                selectedRequest.IsAccepted = true;
                selectedRequest.IsDeclined = false;
                selectedRequest.LastModified = DateTime.Now;
                selectedRequest.ModifiedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
                

                LMOContructorWindow creationWindow = new LMOContructorWindow(SelectedRequest);
                creationWindow.Owner = Application.Current.MainWindow;
                creationWindow.ShowDialog();

                if (creationWindow.wasCancelled)
                {
                    return;
                }
                else
                {
                    selectedRequest.AcceptedBy = App.currentUser.FirstName;
                    selectedRequest.ResultingLMO = creationWindow.constructLMO.Name;
                    selectedRequest.Status = String.Format("Accepted by {0} - {1}", selectedRequest.AcceptedBy, selectedRequest.ResultingLMO);
                }

                
                if (DatabaseHelper.Update(selectedRequest))
                {
                    GetRequests();
                    OnPropertyChanged("SelectedRequest");
                    SelectedRequestChanged?.Invoke(this, new EventArgs());
                    SelectedRequest = Requests.First();
                }
                else
                {
                    MessageBox.Show("Failed to update the request.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            if (!App.currentUser.CanApproveRequests)
            {
                MessageBox.Show("You do not have permission to authorise requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (!selectedRequest.isProductionApproved || !selectedRequest.isSchedulingApproved)
            {
                MessageBox.Show("Cannot approve without both scheduling and production confirmation.", "Pending input", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return;
        }

        public void DeclineRequest()
        {
            if (!App.currentUser.CanApproveRequests)
            {
                MessageBox.Show("You do not have permission to decline requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (String.IsNullOrEmpty(selectedRequest.DeclinedReason))
            {
                MessageBox.Show("You must enter a reason for declining the request.", "Information required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            selectedRequest.IsAccepted = false;
            selectedRequest.IsDeclined = true;
            selectedRequest.LastModified = DateTime.Now;
            selectedRequest.ModifiedBy = String.Format("{0} {1}", App.currentUser.FirstName, App.currentUser.LastName);
            selectedRequest.Status = String.Format("Declined - {0}", selectedRequest.DeclinedReason);
            if (DatabaseHelper.Update(selectedRequest))
            {
                MessageBox.Show("You have declined this request.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                GetRequests();
                OnPropertyChanged("SelectedRequest");
                SelectedRequestChanged?.Invoke(this, new EventArgs());
                SelectedRequest = Requests.First();
            }
            else
            {
                MessageBox.Show("Failed to update the request.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        }

        public void GetRequests()
        {
            var requests = DatabaseHelper.Read<Request>().ToList();
            Requests.Clear();

            foreach (var request in requests)
            {
                Requests.Add(request);
            }
        }
    }
}
