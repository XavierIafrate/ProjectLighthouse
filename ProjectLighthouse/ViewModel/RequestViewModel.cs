using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class RequestViewModel : BaseViewModel
    {
        #region Variables
        public ObservableCollection<Request> Requests { get; set; }
        public ObservableCollection<Request> FilteredRequests { get; set; }
        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterRequests(value);
                if (FilteredRequests.Count > 0)
                {
                    SelectedRequest = FilteredRequests.First();
                    CardVis = Visibility.Visible;
                }
                else
                {
                    CardVis = Visibility.Hidden;
                }

            }
        }

        private Request selectedRequest;
        public Request SelectedRequest
        {
            get { return selectedRequest; }
            set
            {
                selectedRequest = value;
                OnPropertyChanged("SelectedRequest");
                SelectedRequestChanged?.Invoke(this, new EventArgs());

                LoadRequestCard(value);
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

        public bool UpdateButtonEnabled { get; set; }

        private string purchaseRef;
        public string PurchaseRef
        {
            get { return purchaseRef; }
            set
            {
                purchaseRef = value;
                if (value == null)
                {
                    return;
                }
                if (value.Length > 3)
                {
                    UpdateButtonEnabled = value.Substring(0, 3) == "POR";
                }
                else
                {
                    UpdateButtonEnabled = false;
                }
                OnPropertyChanged("UpdateButtonEnabled");
                OnPropertyChanged("PurchaseRef");
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

        private bool canEditRequirements;
        public bool CanEditRequirements
        {
            get { return canEditRequirements; }
            set 
            { 
                canEditRequirements = value;
                OnPropertyChanged("CanEditRequirements");
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

        private Visibility editcontrolsVis;
        public Visibility EditControlsVis
        {
            get { return editcontrolsVis; }
            set 
            { 
                editcontrolsVis = value;
                OnPropertyChanged("EditControlsVis");
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

        private Visibility cardVis;
        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged("CardVis");
                if (value == Visibility.Visible)
                {
                    NothingVis = Visibility.Hidden;
                    return;
                }
                NothingVis = Visibility.Visible;
            }
        }

        private Visibility nothingVis;
        public Visibility NothingVis
        {
            get { return nothingVis; }
            set
            {
                nothingVis = value;
                OnPropertyChanged("NothingVis");
            }
        }

        private Visibility decisionVis;
        public Visibility DecisionVis
        {
            get { return decisionVis; }
            set
            {
                decisionVis = value;
                OnPropertyChanged("DecisionVis");
            }
        }

        private Visibility approvedVis;
        public Visibility ApprovedVis
        {
            get { return approvedVis; }
            set
            {
                approvedVis = value;
                OnPropertyChanged("ApprovedVis");
            }
        }

        private Visibility declinedVis;
        public Visibility DeclinedVis
        {
            get { return declinedVis; }
            set
            {
                declinedVis = value;
                OnPropertyChanged("DeclinedVis");
            }
        }

        public event EventHandler SelectedRequestChanged;

        public UpdatePORefCommand UpdateOrderCommand { get; set; }
        public ApproveRequestCommand ApproveCommand { get; set; }
        public DeclineRequestCommand DeclineCommand { get; set; }

        #endregion

        public RequestViewModel()
        {
            Requests = new ObservableCollection<Request>();
            FilteredRequests = new ObservableCollection<Request>();
            ApproveCommand = new ApproveRequestCommand(this);
            DeclineCommand = new DeclineRequestCommand(this);
            UpdateOrderCommand = new UpdatePORefCommand(this);
            SelectedRequest = new Request();

            approvalControlsVis = App.currentUser.CanApproveRequests ? Visibility.Visible : Visibility.Collapsed;
            GetRequests();
            FilterRequests("All");

            if (FilteredRequests.Count > 0)
                SelectedRequest = FilteredRequests.First();
        }

        public void LoadRequestCard(Request request)
        {
            if (request == null)
                return;

            ModifiedVis = (String.IsNullOrEmpty(request.ModifiedBy)) ? Visibility.Collapsed : Visibility.Visible;

            ApprovalControlsVis = (App.currentUser.CanApproveRequests && request.Status == "Pending approval") ? Visibility.Visible : Visibility.Collapsed;
            EditControlsVis = (App.currentUser.GetFullName() == request.RaisedBy || App.currentUser.CanApproveRequests) ? Visibility.Visible : Visibility.Collapsed;
            DecisionVis = (request.IsDeclined || request.IsAccepted) ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = request.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = request.IsDeclined ? Visibility.Visible : Visibility.Collapsed;
            CanEditRequirements = (!request.IsAccepted && !request.IsDeclined);
            Debug.WriteLine(CanEditRequirements);


            ProductionCheckboxEnabled = (request.Status == "Pending approval" &&
                (App.currentUser.UserRole == "Production" ||
                App.currentUser.UserRole == "admin"));

            SchedulingCheckboxEnabled = (request.Status == "Pending approval" &&
                (App.currentUser.UserRole == "Scheduling" ||
                App.currentUser.UserRole == "admin"));

            DropboxEnabled = (request.Status == "Pending approval" &&
                (App.currentUser.UserRole == "Scheduling" ||
                App.currentUser.UserRole == "admin" ||
                App.currentUser.UserRole == "Production"));

            PurchaseRef = !String.IsNullOrEmpty(request.POReference) ? request.POReference : "POR";
        }

        public void UpdateOrderPurchaseRef()
        {
            var orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(n => n.Name == SelectedRequest.ResultingLMO).ToList();

            if (orders != null)
            {
                LatheManufactureOrder targetOrder = orders.First();
                targetOrder.POReference = PurchaseRef;
                SelectedRequest.POReference = PurchaseRef;
                DatabaseHelper.Update(targetOrder);
                SelectedRequest.LastModified = DateTime.Now;
                SelectedRequest.ModifiedBy = App.currentUser.GetFullName();
                DatabaseHelper.Update(SelectedRequest);
                MessageBox.Show("Successfully updated " + targetOrder.Name, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Could not find Order in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void UpdateRequirements(string notes, int QuantityRequired)
        {
            SelectedRequest.LastModified = DateTime.Now;
            SelectedRequest.ModifiedBy = App.currentUser.GetFullName();

            SelectedRequest.Notes = notes;
            SelectedRequest.QuantityRequired = QuantityRequired;

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
            if (selectedRequest.isProductionApproved && selectedRequest.isSchedulingApproved && App.currentUser.CanApproveRequests)
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


                if (DatabaseHelper.Update(SelectedRequest))
                {
                    EmailHelper.NotifyRequestApproved(SelectedRequest);
                    FilterRequests(SelectedFilter);
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
            if (DatabaseHelper.Update(SelectedRequest))
            {
                MessageBox.Show("You have declined this request.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                EmailHelper.NotifyRequestDeclined(SelectedRequest);
                FilterRequests(SelectedFilter);
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

            Requests = new ObservableCollection<Request>(Requests.OrderByDescending(n => n.DateRaised));
        }

        public void FilterRequests(string filter)
        {
            switch (filter)
            {
                case "All":
                    FilteredRequests = new ObservableCollection<Request>(Requests);
                    break;
                case "Pending":
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => !n.IsAccepted && !n.IsDeclined));
                    break;
                case "Accepted":
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => n.IsAccepted));
                    break;
                case "Declined":
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => n.IsDeclined));
                    break;
                case "My Requests":
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => n.RaisedBy == App.currentUser.GetFullName()));
                    break;
            }
            if (FilteredRequests.Count > 0)
            {
                selectedRequest = FilteredRequests.First();
            }
            OnPropertyChanged("FilteredRequests");
        }
    }
}
