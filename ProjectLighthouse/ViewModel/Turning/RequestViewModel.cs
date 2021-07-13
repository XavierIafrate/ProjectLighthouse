using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class RequestViewModel : BaseViewModel
    {
        #region Variables
        public List<Request> Requests { get; set; }
        public ObservableCollection<Request> FilteredRequests { get; set; }
        public RequestView window { get; set; }

        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                FilterRequests(value);
                //if(window != null && window.filterComboBox.Text != value)
                //    window.filterComboBox.Text = value;
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
        public RequestsToCSVCommand ExportCommand { get; set; }

        #endregion

        public RequestViewModel()
        {
            Debug.WriteLine("Init: RequestsViewModel");
            Requests = new List<Request>();
            FilteredRequests = new ObservableCollection<Request>();
            ApproveCommand = new ApproveRequestCommand(this);
            DeclineCommand = new DeclineRequestCommand(this);
            UpdateOrderCommand = new UpdatePORefCommand(this);
            ExportCommand = new RequestsToCSVCommand(this);
            SelectedRequest = new Request();

            approvalControlsVis = App.CurrentUser.CanApproveRequests ? Visibility.Visible : Visibility.Collapsed;
            GetRequests();
            //SelectedFilter = "Last 14 Days";

            //if (FilteredRequests.Count < 1)
            //    SelectedFilter = "My Requests";
        }

        public void LoadRequestCard(Request request)
        {
            if (request == null)
                return;

            ModifiedVis = (String.IsNullOrEmpty(request.ModifiedBy)) ? Visibility.Collapsed : Visibility.Visible;

            ApprovalControlsVis = (App.CurrentUser.CanApproveRequests && request.Status == "Pending approval") ? Visibility.Visible : Visibility.Collapsed;
            EditControlsVis = (App.CurrentUser.GetFullName() == request.RaisedBy || App.CurrentUser.CanApproveRequests) ? Visibility.Visible : Visibility.Collapsed;
            DecisionVis = (request.IsDeclined || request.IsAccepted) ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = request.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = request.IsDeclined ? Visibility.Visible : Visibility.Collapsed;
            CanEditRequirements = (!request.IsAccepted && !request.IsDeclined);


            ProductionCheckboxEnabled = (request.Status == "Pending approval" &&
                (App.CurrentUser.UserRole == "Production" ||
                App.CurrentUser.UserRole == "admin")
                && App.CurrentUser.CanApproveRequests);

            SchedulingCheckboxEnabled = (request.Status == "Pending approval" &&
                (App.CurrentUser.UserRole == "Scheduling" ||
                App.CurrentUser.UserRole == "admin"));

            DropboxEnabled = (request.Status == "Pending approval" &&
                (App.CurrentUser.UserRole == "Scheduling" ||
                App.CurrentUser.UserRole == "admin" ||
                App.CurrentUser.UserRole == "Production")
                && App.CurrentUser.CanApproveRequests);

            PurchaseRef = !String.IsNullOrEmpty(request.POReference) ? request.POReference : "POR";
        }

        public void UpdateOrderPurchaseRef()
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(n => n.Name == SelectedRequest.ResultingLMO).ToList();

            if (orders != null)
            {
                LatheManufactureOrder targetOrder = orders.First();
                targetOrder.POReference = PurchaseRef;
                SelectedRequest.POReference = PurchaseRef;
                DatabaseHelper.Update(targetOrder);
                SelectedRequest.LastModified = DateTime.Now;
                SelectedRequest.ModifiedBy = App.CurrentUser.GetFullName();
                DatabaseHelper.Update(SelectedRequest);
                MessageBox.Show("Successfully updated " + targetOrder.Name, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Could not find Order in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportRequestsToCSV()
        {
            CSVHelper.WriteListToCSV(FilteredRequests.ToList(), "Lighthouse_Requests");
        }

        public void UpdateRequest()
        {
            SelectedRequest.LastModified = DateTime.Now;
            SelectedRequest.ModifiedBy = String.Format("{0} {1}", App.CurrentUser.FirstName, App.CurrentUser.LastName);
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
            SelectedRequest.ModifiedBy = App.CurrentUser.GetFullName();

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
            if (selectedRequest.isProductionApproved && selectedRequest.isSchedulingApproved && App.CurrentUser.CanApproveRequests)
            {
                selectedRequest.IsAccepted = true;
                selectedRequest.IsDeclined = false;
                selectedRequest.LastModified = DateTime.Now;
                selectedRequest.ModifiedBy = String.Format("{0} {1}", App.CurrentUser.FirstName, App.CurrentUser.LastName);


                LMOContructorWindow creationWindow = new LMOContructorWindow(SelectedRequest);
                creationWindow.Owner = Application.Current.MainWindow;
                creationWindow.ShowDialog();

                if (creationWindow.wasCancelled)
                {
                    return;
                }
                else
                {
                    selectedRequest.AcceptedBy = App.CurrentUser.FirstName;
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
            if (!App.CurrentUser.CanApproveRequests)
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
            if (!App.CurrentUser.CanApproveRequests)
            {
                MessageBox.Show("You do not have permission to decline requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (String.IsNullOrEmpty(SelectedRequest.DeclinedReason))
            {
                MessageBox.Show("You must enter a reason for declining the request.", "Information required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SelectedRequest.IsAccepted = false;
            SelectedRequest.IsDeclined = true;
            SelectedRequest.LastModified = DateTime.Now;
            SelectedRequest.ModifiedBy = String.Format("{0} {1}", App.CurrentUser.FirstName, App.CurrentUser.LastName);
            selectedRequest.Status = String.Format("Declined - {0}", SelectedRequest.DeclinedReason);
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
            Requests.Clear();
            Requests = DatabaseHelper.Read<Request>().OrderByDescending(n => n.DateRaised).ToList();
        }

        public void FilterRequests(string filter)
        {
            switch (filter)
            {
                case "Last 14 Days":
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => n.DateRaised.AddDays(14) > DateTime.Now));
                    break;
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
                    FilteredRequests = new ObservableCollection<Request>(Requests.Where(n => n.RaisedBy == App.CurrentUser.GetFullName()));
                    break;
            }
            if (FilteredRequests.Count > 0)
            {
                SelectedRequest = FilteredRequests.First();
            }
            OnPropertyChanged("FilteredRequests");
            OnPropertyChanged("SelectedFilter");
        }
    }
}
