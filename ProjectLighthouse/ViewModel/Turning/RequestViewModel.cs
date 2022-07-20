using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.View.HelperWindows;
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
        public List<LatheManufactureOrder> ActiveOrders { get; set; }
        public List<LatheManufactureOrderItem> ActiveOrderItems { get; set; }
        public List<LatheManufactureOrderItem> RecommendedManifest { get; set; }

        public List<Note> FilteredNotes { get; set; }
        public List<Note> Notes { get; set; }

        private double targetRuntime;

        public double TargetRuntime
        {
            get { return targetRuntime; }
            set
            {
                targetRuntime = value;
                OnPropertyChanged();

                if (SelectedRequest.Product != null)
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
                OnPropertyChanged();

                LoadRequestCard(value);
            }
        }

        private TurnedProduct selectedRequestProduct;

        public TurnedProduct SelectedRequestProduct
        {
            get { return selectedRequestProduct; }
            set 
            { 
                selectedRequestProduct = value;
                if (value.ProductName != null)
                {
                    selectedRequestProduct.Group = ProductGroups.Find(x => selectedRequestProduct.ProductName.StartsWith(x.Name));
                }
                OnPropertyChanged();
            }
        }

        public List<Product> ProductGroups { get; set; }

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
                OnPropertyChanged();
            }
        }

        private bool dropboxEnabled;
        public bool DropboxEnabled
        {
            get { return dropboxEnabled; }
            set
            {
                dropboxEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool canEditRequirements;
        public bool CanEditRequirements
        {
            get { return canEditRequirements; }
            set
            {
                canEditRequirements = value;
                OnPropertyChanged();
            }
        }
        #region Visibilities

        private Visibility approvalControlsVis;
        public Visibility ApprovalControlsVis
        {
            get { return approvalControlsVis; }
            set
            {
                approvalControlsVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility editcontrolsVis;
        public Visibility EditControlsVis
        {
            get { return editcontrolsVis; }
            set
            {
                editcontrolsVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility modifiedVis;
        public Visibility ModifiedVis
        {
            get { return modifiedVis; }
            set
            {
                modifiedVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility cleaningVis;

        public Visibility CleaningVis
        {
            get { return cleaningVis; }
            set
            {
                cleaningVis = value;
                OnPropertyChanged();
            }
        }


        private Visibility cardVis;
        public Visibility CardVis
        {
            get { return cardVis; }
            set
            {
                cardVis = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private Visibility decisionVis;
        public Visibility DecisionVis
        {
            get { return decisionVis; }
            set
            {
                decisionVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility approvedVis;
        public Visibility ApprovedVis
        {
            get { return approvedVis; }
            set
            {
                approvedVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility declinedVis;
        public Visibility DeclinedVis
        {
            get { return declinedVis; }
            set
            {
                declinedVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility mergeVis;
        public Visibility MergeVis
        {
            get { return mergeVis; }
            set
            {
                mergeVis = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public event EventHandler SelectedRequestChanged;

        public UpdatePORefCommand UpdateOrderCommand { get; set; }
        public ApproveRequestCommand ApproveCommand { get; set; }
        public DeclineRequestCommand DeclineCommand { get; set; }
        public RequestsToCSVCommand ExportCommand { get; set; }
        public MergeRequestToOrderCommand MergeCommand { get; set; }
        public ViewMakeOrBuyCommand ViewMakeOrBuyCommand { get; set; }
        public UpdateRequestCommand UpdateRequestCmd { get; set; }
        public EditProductCommand ModifyProductCommand { get; set; }

        #endregion

        public RequestViewModel()
        {
            RecommendedManifest = new();
            Requests = new();
            FilteredRequests = new();
            ApproveCommand = new(this);
            DeclineCommand = new(this);
            UpdateOrderCommand = new(this);
            ExportCommand = new(this);
            MergeCommand = new(this);
            ViewMakeOrBuyCommand = new(this);
            ModifyProductCommand = new(this);
            UpdateRequestCmd = new(this);
            Notes = new();
            Notes = DatabaseHelper.Read<Note>().ToList();

            SelectedRequest = new();

            TargetRuntime = 5;

            approvalControlsVis = App.CurrentUser.CanApproveRequests
                ? Visibility.Visible
                : Visibility.Collapsed;


            GetRequests();

            Products = DatabaseHelper.Read<TurnedProduct>();
            ProductGroups = DatabaseHelper.Read<Product>();

            ActiveOrders = DatabaseHelper.Read<LatheManufactureOrder>()
                .Where(x => x.State < OrderState.Complete)
                .ToList();
            string[] activeOrders = ActiveOrders.Select(x => x.Name).ToArray();
            ActiveOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>()
                .Where(x => activeOrders.Contains(x.AssignedMO))
                .ToList();

            for (int i = 0; i < ActiveOrders.Count; i++)
            {
                ActiveOrders[i].OrderItems = ActiveOrderItems.Where(x => x.AssignedMO == ActiveOrders[i].Name).ToList();
            }

            CheckForAppendOppurtunities();

            SelectedFilter = "Last 14 Days";
        }

        public void EditProduct()
        {
            EditProductWindow window = new(SelectedRequestProduct);
            window.ShowDialog();
        }

        private void CheckForAppendOppurtunities()
        {
            //List<TurnedProduct> productsOnOrder = new();
            //for (int i = 0; i < ActiveOrderItems.Count; i++)
            //{
            //    TurnedProduct product = Products.Find(x => x.ProductName == ActiveOrderItems[i].ProductName);
            //    if (product != null)
            //    {
            //        product.IsAlreadyOnOrder = true;
            //        product.OrderReference = ActiveOrderItems[i].AssignedMO;
            //        productsOnOrder.Add(product);
            //    }
            //}

            //for (int i = 0; i < Requests.Count; i++)
            //{
            //    if (Requests[i].Status != "Pending approval")
            //    {
            //        continue;
            //    }
            //    TurnedProduct requestedProduct = Products.Find(x => x.ProductName == Requests[i].Product);
            //    for (int j = 0; j < productsOnOrder.Count; j++)
            //    {
            //        if (requestedProduct.IsScheduleCompatible(productsOnOrder[j]))
            //        {
            //            Requests[i].CanAppend = true;
            //            Requests[i].ExistingOrder = productsOnOrder[j].OrderReference;
            //        }
            //        if (requestedProduct.ProductName == productsOnOrder[j].ProductName)
            //        {
            //            Requests[i].UpdateOrder = true;
            //        }
            //    }
            //}
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

            MergeVis = request.CanAppend
                ? Visibility.Visible
                : Visibility.Collapsed;

            ApprovalControlsVis = (App.CurrentUser.CanApproveRequests && request.Status == "Pending approval") ? Visibility.Visible : Visibility.Collapsed;
            EditControlsVis = (App.CurrentUser.GetFullName() == request.RaisedBy) && !request.IsAccepted && !request.IsDeclined
                ? Visibility.Visible 
                : Visibility.Collapsed;
            DecisionVis = (request.IsDeclined || request.IsAccepted) ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = request.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = request.IsDeclined ? Visibility.Visible : Visibility.Collapsed;
            CanEditRequirements = !request.IsAccepted && !request.IsDeclined;

            DropboxEnabled = request.Status == "Pending approval" &&
                App.CurrentUser.Role >= UserRole.Scheduling
                && App.CurrentUser.CanApproveRequests;

            PurchaseRef = !string.IsNullOrEmpty(request.POReference) ? request.POReference : "POR";


            if (!string.IsNullOrEmpty(SelectedRequest.Product))
            {
                SelectedRequestProduct = Products.Find(x => x.ProductName == SelectedRequest.Product);
            }

            if (request.Product != null)
            {
                FilteredNotes = null;
                OnPropertyChanged(nameof(FilteredNotes));
                FilteredNotes = Notes.Where(x => x.DocumentReference == $"r{request.Id:0}").ToList();
                OnPropertyChanged(nameof(FilteredNotes));
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
                TimeSpan.FromDays(Math.Round(TargetRuntime)),
                SelectedRequest.DateRequired);
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

        public void SelectById(int id)
        {
            Request target = Requests.Find(x => x.Id == id);
            LoadRequestCard(target);
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

        public void ShowMakeOrBuy()
        {
            LMOContructorWindow creationWindow = new(request: SelectedRequest, preselectedItems: RecommendedManifest, withAuthority:false);
            creationWindow.Owner = Application.Current.MainWindow;
            creationWindow.ShowDialog();
        }

        public void ApproveRequest(bool merge = false)
        {
            if (!merge)
            {
                LMOContructorWindow creationWindow = new(request: SelectedRequest, preselectedItems: RecommendedManifest);
                creationWindow.Owner = Application.Current.MainWindow;
                creationWindow.ShowDialog();

                if (creationWindow.Cancelled)
                {
                    return;
                }
                SelectedRequest.ResultingLMO = creationWindow.NewOrder.Name;
            }
            else
            {
                MessageBox.Show("Merging not implemented yet", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            SelectedRequest.MarkAsAccepted();

            User ToNotify = DatabaseHelper.Read<User>().Find(x => x.GetFullName() == SelectedRequest.RaisedBy);

            Notification not = new(
                to: ToNotify.UserName,
                from: App.CurrentUser.UserName,
                header: "Request Approved",
                body: $"Your request for {SelectedRequest.QuantityRequired:#,##0} pcs of {SelectedRequest.Product} has been approved! Please update Lighthouse with the Purchase Reference.",
                toastAction: $"viewManufactureOrder:{SelectedRequest.ResultingLMO}");

            if (not.TargetUser != not.Origin)
            {
                DatabaseHelper.Insert(not);
            }

            if (DatabaseHelper.Update(SelectedRequest))
            {
                int id = SelectedRequest.Id;
                new ToastContentBuilder()
                   .AddText($"Order {SelectedRequest.ResultingLMO} created.")
                   .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                   .AddText("You have successfully approved this request.")
                   .Show();
                FilterRequests(SelectedFilter);
                foreach(Request request in FilteredRequests)
                {
                    if (request.Id == id)
                    {
                        SelectedRequest = request; 
                        OnPropertyChanged(nameof(SelectedRequest));
                    }
                }
            }
            else
            {
                new ToastContentBuilder()
                   .AddText("An error occurred.")
                   .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                   .AddText("Lighthouse encountered an error while updating request")
                   .Show();
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
            Requests = DatabaseHelper.Read<Request>().ToList();
        }

        public void FilterRequests(string filter)
        {
            List<Request> requests = new(Requests);
            switch (filter)
            {
                case "Last 14 Days":
                    requests = requests.Where(n => n.DateRaised.AddDays(14) > DateTime.Now).ToList();
                    break;
                case "All":
                    // no filter
                    break;
                case "Pending":
                    requests = requests.Where(n => !n.IsAccepted && !n.IsDeclined).ToList();
                    break;
                case "Accepted":
                    requests = requests.Where(n => n.IsAccepted).ToList();
                    break;
                case "Declined":
                    requests = requests.Where(n => n.IsDeclined).ToList();
                    break;
                case "My Requests":
                    requests = requests.Where(n => n.RaisedBy == App.CurrentUser.GetFullName()).ToList();
                    break;
            }

            FilteredRequests = new ObservableCollection<Request>(requests.OrderByDescending(x => x.DateRaised));

            if (FilteredRequests.Count > 0)
            {
                SelectedRequest = FilteredRequests.First();
            }

            OnPropertyChanged("FilteredRequests");
            OnPropertyChanged("SelectedFilter");
        }
    }
}
