using ProjectLighthouse.Model;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class RequestViewModel : BaseViewModel
    {
        #region Variables
        public List<Request> Requests { get; set; }
        public ObservableCollection<Request> FilteredRequests { get; set; }
        public List<TurnedProduct> Products { get; set; }
        public List<LatheManufactureOrderItem> RecommendedManifest { get; set; }
        private double targetRuntime;

        public double TargetRuntime
        {
            get { return targetRuntime; }
            set
            {
                targetRuntime = value;
                OnPropertyChanged("TargetRuntime");

                if (SelectedRequest.Status == "Pending approval" && SelectedRequest.Product != null)
                {
                    LoadRecommendedOrder();
                }
            }
        }

        public RequestView Window { get; set; }

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

                LoadRequestCard(value);
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
                UpdateButtonEnabled = App.CurrentUser.UserRole is "Purchasing" or "admin";
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

        private Visibility cleaningVis;

        public Visibility CleaningVis
        {
            get { return cleaningVis; }
            set
            {
                cleaningVis = value;
                OnPropertyChanged("CleaningVis");
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
            RecommendedManifest = new();
            Requests = new List<Request>();
            FilteredRequests = new ObservableCollection<Request>();
            ApproveCommand = new ApproveRequestCommand(this);
            DeclineCommand = new DeclineRequestCommand(this);
            UpdateOrderCommand = new UpdatePORefCommand(this);
            ExportCommand = new RequestsToCSVCommand(this);
            SelectedRequest = new Request();
            Products = DatabaseHelper.Read<TurnedProduct>();

            TargetRuntime = 5;

            approvalControlsVis = App.CurrentUser.CanApproveRequests ? Visibility.Visible : Visibility.Collapsed;
            GetRequests();
        }

        public void LoadRequestCard(Request request)
        {
            if (request == null)
            {
                return;
            }

            CleaningVis = request.CleanCustomerRequirement
                ? Visibility.Visible
                : Visibility.Collapsed;

            ModifiedVis = string.IsNullOrEmpty(request.ModifiedBy)
                ? Visibility.Collapsed
                : Visibility.Visible;

            ApprovalControlsVis = (App.CurrentUser.CanApproveRequests && request.Status == "Pending approval") ? Visibility.Visible : Visibility.Collapsed;
            EditControlsVis = (App.CurrentUser.GetFullName() == request.RaisedBy || App.CurrentUser.CanApproveRequests) ? Visibility.Visible : Visibility.Collapsed;
            DecisionVis = (request.IsDeclined || request.IsAccepted) ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = request.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = request.IsDeclined ? Visibility.Visible : Visibility.Collapsed;
            CanEditRequirements = !request.IsAccepted && !request.IsDeclined;

            DropboxEnabled = request.Status == "Pending approval" &&
                (App.CurrentUser.UserRole == "Scheduling" ||
                App.CurrentUser.UserRole == "admin" ||
                App.CurrentUser.UserRole == "Production")
                && App.CurrentUser.CanApproveRequests;

            PurchaseRef = !string.IsNullOrEmpty(request.POReference) ? request.POReference : "POR";

            if (request.Status == "Pending approval" && request.Product != null)
            {
                LoadRecommendedOrder();
            }
        }

        private void LoadRecommendedOrder()
        {
            RecommendedManifest.Clear();
            TurnedProduct requiredProduct = Products.First(p => p.ProductName == SelectedRequest.Product);

            RecommendedManifest = RequestsEngine.GetRecommendedOrderItems(Products,
                requiredProduct,
                SelectedRequest.QuantityRequired,
                TimeSpan.FromDays(Math.Round(TargetRuntime)));
            OnPropertyChanged("RecommendedManifest");
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
            SelectedRequest.ModifiedBy = App.CurrentUser.GetFullName();

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
            if (!App.CurrentUser.CanApproveRequests)
            {
                MessageBox.Show("You do not have permission to authorise requests.\nHow can you even see the buttons??",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            LMOContructorWindow creationWindow = new(SelectedRequest, TargetRuntime, Products);
            creationWindow.Owner = Application.Current.MainWindow;
            creationWindow.ShowDialog();

            if (creationWindow.wasCancelled)
            {
                return;
            }

            SelectedRequest.ResultingLMO = creationWindow.NewOrder.Name;
            SelectedRequest.MarkAsAccepted();
            Debug.WriteLine($"Requested Product: {SelectedRequest.Product}");


            if (DatabaseHelper.Update(SelectedRequest))
            {
                Debug.WriteLine($"Emailing about: {SelectedRequest.Product}");
                Request tmp = (Request)SelectedRequest.Clone();
                Task.Run(() => EmailHelper.NotifyRequestApproved(tmp));
                //Task.Run(() => EmailHelper.NotifyNewOrder(creationWindow.NewOrder, creationWindow.NewOrderItems));

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


        public void DeclineRequest()
        {
            if (!App.CurrentUser.CanApproveRequests)
            {
                MessageBox.Show("You do not have permission to decline requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(SelectedRequest.DeclinedReason))
            {
                MessageBox.Show("You must enter a reason for declining the request.", "Information required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedRequest.MarkAsDeclined();

            if (DatabaseHelper.Update(SelectedRequest))
            {
                MessageBox.Show("You have declined this request.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Request tmp = (Request)SelectedRequest.Clone();
                Task.Run(() => EmailHelper.NotifyRequestDeclined(tmp));

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
