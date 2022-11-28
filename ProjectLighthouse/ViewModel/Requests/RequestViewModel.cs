using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.View.HelperWindows;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.View.Requests;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Orders;
using ProjectLighthouse.ViewModel.Commands.Requests;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Requests
{
    public class RequestViewModel : BaseViewModel
    {
        #region Variables
        public List<Request> Requests { get; set; }
        public ObservableCollection<Request> FilteredRequests { get; set; }
        public List<TurnedProduct> Products { get; set; }
        public List<LatheManufactureOrder> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
        public List<LatheManufactureOrderItem> RecommendedManifest { get; set; }

        public List<Note> FilteredNotes { get; set; }
        public List<Note> Notes { get; set; }

        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set
            {
                newMessage = value;
                OnPropertyChanged();
            }
        }

        private string searchString;

        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                FilterRequests(search: true);
                OnPropertyChanged();
            }
        }

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
                FilterRequests();
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
                if (value != null)
                {
                    // TODO check
                    //selectedRequestProduct.Group = ProductGroups.Find(x => selectedRequestProduct.ProductName.StartsWith(x.Name));
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
                UpdateButtonEnabled = App.CurrentUser.Role is UserRole.Purchasing or UserRole.Administrator;
                OnPropertyChanged(nameof(UpdateButtonEnabled));
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


        private bool showingRequest;

        public bool ShowingRequest
        {
            get { return showingRequest; }
            set { showingRequest = value; OnPropertyChanged(); }
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

        private bool dataRequired;
        public bool DataRequired
        {
            get { return dataRequired; }
            set
            {
                dataRequired = value;
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
        public SendMessageCommand SendMessageCommand { get; set; }

        #endregion

        public RequestViewModel()
        {
            InitialiseVariables();
            LoadData();
            CheckForAppendOppurtunities();
        }

        private void InitialiseVariables()
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
            SendMessageCommand = new(this);
            Notes = new();
            SelectedRequest = new();
            ApprovalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest)
                ? Visibility.Visible
                : Visibility.Collapsed;
            TargetRuntime = 5;
        }

        public void EditProduct()
        {
            EditProductWindow window = new(SelectedRequestProduct);
            window.ShowDialog();
        }

        //TODO Auto Append stuff
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

        public void LoadRequestCard(Request? request)
        {
            if (request == null)
            {
                ShowingRequest = false;
                return;
            }
            else if (request.Product == null)
            {
                ShowingRequest = false;
                return;
            }
            ShowingRequest = true;

            approvalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest)
                ? Visibility.Visible
                : Visibility.Collapsed;


            SelectedRequestProduct = Products.Find(x => x.ProductName == SelectedRequest.Product);
            SelectedRequestProduct.ValidateAll();

            // TODO throw 
            if (SelectedRequestProduct == null)
            {
                MessageBox.Show($"{request.Product} Not found, please notify an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            ApprovalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest) && request.Status == "Pending approval" && SelectedRequestProduct.NoErrors 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            EditControlsVis = App.CurrentUser.GetFullName() == request.RaisedBy && !request.IsAccepted && !request.IsDeclined
                ? Visibility.Visible
                : Visibility.Collapsed;

            SelectedRequestProduct.ValidateAll();
            DataRequired = SelectedRequestProduct.HasErrors && !request.IsDeclined && !request.IsAccepted;

            DecisionVis = request.IsDeclined || request.IsAccepted ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = request.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = request.IsDeclined ? Visibility.Visible : Visibility.Collapsed;

            CanEditRequirements = !request.IsAccepted && !request.IsDeclined;

            DropboxEnabled = request.Status == "Pending approval" &&
                App.CurrentUser.Role >= UserRole.Scheduling
                && App.CurrentUser.HasPermission(PermissionType.ApproveRequest);

            PurchaseRef = !string.IsNullOrEmpty(request.POReference) ? request.POReference : "POR";

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

            OnPropertyChanged(nameof(RecommendedManifest));
        }


        //TODO remove
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
                OnPropertyChanged(nameof(SelectedRequest));
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
                OnPropertyChanged(nameof(SelectedRequest));
                ModifiedVis = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Failed to update", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShowMakeOrBuy()
        {
            // TODO this
            throw new NotImplementedException();
            //LMOContructorWindow creationWindow = new(request: SelectedRequest, preselectedItems: RecommendedManifest, withAuthority: false)
            //{
            //    Owner = Application.Current.MainWindow
            //};
            //creationWindow.ShowDialog();
        }

        public void ApproveRequest(bool merge = false)
        {
            // TODO this
            if (!merge)
            {
                //LMOContructorWindow creationWindow = new(request: SelectedRequest, preselectedItems: RecommendedManifest)
                //{
                //    Owner = Application.Current.MainWindow
                //};
                //creationWindow.ShowDialog();

                //if (creationWindow.Cancelled)
                //{
                //    return;
                //}
                //SelectedRequest.ResultingLMO = creationWindow.NewOrder.Name;
            }
            else
            {
                MessageBox.Show("Merging not implemented yet", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedRequest.MarkAsAccepted();

            //TODO refactor
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
                MessageBox.Show("An error occurred while updating the request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int id = SelectedRequest.Id;
            new ToastContentBuilder()
                .AddText($"Order {SelectedRequest.ResultingLMO} created.")
                .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                .AddText("You have successfully approved this request.")
                .Show();

            FilterRequests();

            foreach (Request request in FilteredRequests)
            {
                if (request.Id == id)
                {
                    SelectedRequest = request;
                    OnPropertyChanged(nameof(SelectedRequest));
                }
            }

        }

        public void DeclineRequest()
        {
            if (!App.CurrentUser.HasPermission(PermissionType.ApproveRequest))
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

                FilterRequests();
                OnPropertyChanged(nameof(SelectedRequest));
                SelectedRequest = Requests.First();
            }
            else
            {
                MessageBox.Show("Failed to update the request.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadData()
        {
            Notes = DatabaseHelper.Read<Note>().ToList();
            Products = DatabaseHelper.Read<TurnedProduct>();
            ProductGroups = DatabaseHelper.Read<Product>();

            TargetRuntime = 5;

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();
            for (int i = 0; i < Orders.Count; i++)
            {
                Orders[i].OrderItems = OrderItems.Where(x => x.AssignedMO == Orders[i].Name).ToList();
            }

            List<Request> requests = DatabaseHelper.Read<Request>();
            for(int i = 0; i < requests.Count; i++)
            {
                if (!string.IsNullOrEmpty(requests[i].ResultingLMO))
                {
                    requests[i].SubsequentOrder = Orders.Find(x => x.Name == requests[i].ResultingLMO);
                }
            }

            Requests = requests;

            SelectedFilter = "Active";
        }

        public void FilterRequests(bool search = false)
        {
            List<Request> requests = new(Requests);

            if (search)
            {
                if (string.IsNullOrWhiteSpace(SearchString))
                {
                    SelectedFilter = "Active";
                    return;
                }

                selectedFilter = "All";
                OnPropertyChanged(nameof(SelectedFilter));

                requests = requests
                    .Where(x =>
                        (x.POReference ?? "").ToUpperInvariant().Contains(searchString.ToUpperInvariant()) ||
                        x.Product.Contains(searchString.ToUpperInvariant()))
                    .ToList();
            }
            else
            {
                searchString = null;
                OnPropertyChanged(nameof(SearchString));

                switch (SelectedFilter)
                {
                    case "All":
                        break;
                    case "Active":
                        requests = requests.Where(x => 
                        (!x.IsDeclined && !x.IsAccepted) || 
                        (x.SubsequentOrder.State < OrderState.Complete && !string.IsNullOrEmpty(x.ResultingLMO)))
                            .ToList();
                        break;
                    case "Last 14 Days":
                        requests = requests.Where(n => n.DateRaised.AddDays(14) > DateTime.Now).ToList();
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
                    default:
                        break;
                }
            }

            FilteredRequests = new ObservableCollection<Request>(requests.OrderByDescending(x => x.DateRaised));

            if (FilteredRequests.Count > 0)
            {
                SelectedRequest = FilteredRequests.First();
            }
            else
            {
                ShowingRequest = false;
            }

            OnPropertyChanged(nameof(FilteredRequests));
            OnPropertyChanged(nameof(SelectedFilter));
        }

        public void SendMessage()
        {
            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"r{SelectedRequest.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            _ = DatabaseHelper.Insert(newNote);

            List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasPermission(PermissionType.ApproveRequest)).Select(x => x.UserName));
            otherUsers.Add(App.NotificationsManager.users.Find(x => x.GetFullName() == SelectedRequest.RaisedBy)!.UserName);

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], App.CurrentUser.UserName, $"Comment: Request #{SelectedRequest.Id:0}", $"{App.CurrentUser.FirstName} left a comment: {NewMessage}");
                _ = DatabaseHelper.Insert(newNotification);
            }

            Notes.Add(newNote);
            LoadRequestCard(SelectedRequest);

            NewMessage = "";
        }
    }
}
