using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.View.Orders;
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
using System.Threading.Tasks;
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

        private string frequencyAnalysis;

        public string FrequencyAnalysis
        {
            get { return frequencyAnalysis; }
            set 
            { 
                frequencyAnalysis = value;
                OnPropertyChanged();
            }
        }


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

                if (SelectedRequest is null) return;
                if (SelectedRequest.Product != null)
                {
                    LoadRecommendedOrder();
                }
            }
        }

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

        private Request? selectedRequest;
        public Request? SelectedRequest
        {
            get { return selectedRequest; }
            set
            {
                selectedRequest = value;
                OnPropertyChanged();
                LoadRequestCard();
            }
        }

        private TurnedProduct? selectedRequestProduct;
        public TurnedProduct? SelectedRequestProduct
        {
            get { return selectedRequestProduct; }
            set
            {
                selectedRequestProduct = value;
                OnPropertyChanged();
            }
        }

        private ProductGroup? selectedRequestProductGroup;
        public ProductGroup? SelectedRequestProductGroup
        {
            get { return selectedRequestProductGroup; }
            set
            {
                selectedRequestProductGroup = value;
                OnPropertyChanged();
            }
        }

        private Product? selectedRequestMainProduct;
        public Product? SelectedRequestMainProduct
        {
            get { return selectedRequestMainProduct; }
            set
            {
                selectedRequestMainProduct = value;
                OnPropertyChanged();
            }
        }


        public List<ProductGroup> ProductGroups { get; set; }
        public List<Product> MainProducts { get; set; }
        public List<MaterialInfo> Materials { get; set; }

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

        #region Commands

        public ApproveRequestCommand ApproveCommand { get; set; }
        public DeclineRequestCommand DeclineCommand { get; set; }
        public RequestsToCSVCommand ExportCommand { get; set; }
        public MergeRequestToOrderCommand MergeCommand { get; set; }
        public ViewMakeOrBuyCommand ViewMakeOrBuyCommand { get; set; }
        public UpdateRequestCommand UpdateRequestCmd { get; set; }
        public EditProductCommand ModifyProductCommand { get; set; }
        public SendMessageCommand SendMessageCommand { get; set; }

        #endregion
        #endregion

        public RequestViewModel()
        {
            InitialiseVariables();
            LoadData();
        }

        private void InitialiseVariables()
        {
            RecommendedManifest = new();
            Requests = new();
            FilteredRequests = new();

            ApproveCommand = new(this);
            DeclineCommand = new(this);
            ExportCommand = new(this);
            MergeCommand = new(this);
            ViewMakeOrBuyCommand = new(this);
            ModifyProductCommand = new(this);
            UpdateRequestCmd = new(this);
            SendMessageCommand = new(this);

            Notes = new();

            ApprovalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest)
                ? Visibility.Visible
                : Visibility.Collapsed;
            TargetRuntime = 5;
        }

        public void EditProduct()
        {
            List<ProductGroup> groups = DatabaseHelper.Read<ProductGroup>();

            AddTurnedProductWindow window = new(groups, SelectedRequestProduct)
            {
                Owner = App.MainViewModel.MainWindow
            };

            window.ShowDialog();
            if (!window.SaveExit)
            {
                return;
            }

            LoadRequestCard();
        }

        public void LoadRequestCard()
        {
            if (SelectedRequest is null) return;

            approvalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest)
                ? Visibility.Visible
                : Visibility.Collapsed;


            SelectedRequestProduct = Products.Find(x => x.ProductName == SelectedRequest.Product);

            if (SelectedRequestProduct == null)
            {
                MessageBox.Show($"Product '{SelectedRequest.Product}' not found, please notify an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SelectedRequestProductGroup = null;
                SelectedRequestMainProduct = null;
                return;
            }

            if (SelectedRequestProduct.GroupId is not null)
            {
                SelectedRequestProductGroup = ProductGroups.Find(x => x.Id == SelectedRequestProduct.GroupId);
                if (SelectedRequestProductGroup == null)
                {
                    MessageBox.Show($"Group ID '{SelectedRequestProduct.GroupId}' not found, please notify an administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SelectedRequestMainProduct = null;
                    return;
                }

                SelectedRequestMainProduct = MainProducts.Find(x => x.Id == SelectedRequestProductGroup.ProductId); // Can be null
            }
            else
            {
                SelectedRequestProductGroup = null;
                SelectedRequestMainProduct = null;
            }

            SelectedRequestProduct.ValidateAll();
            DataRequired = SelectedRequestProduct.HasErrors;

            ModifiedVis = string.IsNullOrEmpty(SelectedRequest.ModifiedBy)
                ? Visibility.Collapsed
                : Visibility.Visible;

            MergeVis = SelectedRequest.CanAppend
                ? Visibility.Visible
                : Visibility.Collapsed;

            ApprovalControlsVis = App.CurrentUser.HasPermission(PermissionType.ApproveRequest) && SelectedRequest.Status == "Pending approval"
                ? Visibility.Visible
                : Visibility.Collapsed;

            EditControlsVis = App.CurrentUser.GetFullName() == SelectedRequest.RaisedBy && !SelectedRequest.IsAccepted && !SelectedRequest.IsDeclined
                ? Visibility.Visible
                : Visibility.Collapsed;

            SelectedRequestProduct.ValidateForOrder();
            DataRequired = SelectedRequestProduct.HasErrors && !SelectedRequest.IsDeclined && !SelectedRequest.IsAccepted;

            DecisionVis = SelectedRequest.IsDeclined || SelectedRequest.IsAccepted ? Visibility.Collapsed : Visibility.Visible;
            ApprovedVis = SelectedRequest.IsAccepted ? Visibility.Visible : Visibility.Collapsed;
            DeclinedVis = SelectedRequest.IsDeclined ? Visibility.Visible : Visibility.Collapsed;

            CanEditRequirements = !SelectedRequest.IsAccepted && !SelectedRequest.IsDeclined;

            DropboxEnabled = SelectedRequest.Status == "Pending approval" &&
                App.CurrentUser.Role >= UserRole.Scheduling
                && App.CurrentUser.HasPermission(PermissionType.ApproveRequest);

            PurchaseRef = !string.IsNullOrEmpty(SelectedRequest.POReference) ? SelectedRequest.POReference : "POR";

            if (SelectedRequest.Product is not null)
            {
                FilteredNotes = null;
                OnPropertyChanged(nameof(FilteredNotes));
                FilteredNotes = Notes.Where(x => x.DocumentReference == $"r{SelectedRequest.Id:0}").ToList();
                OnPropertyChanged(nameof(FilteredNotes));
                LoadRecommendedOrder();
            }

            if (SelectedRequestMainProduct is not null)
            {
                Task.Run(() => GenerateAnalysis());
            }
        }


        void GenerateAnalysis() 
        {
            if (SelectedRequest is null) return;
            if (SelectedRequestProduct is null) return;

            ProductGroup? group = ProductGroups.Find(x => x.Id == SelectedRequestProduct.GroupId);

            if (group is null)
            {
                FrequencyAnalysis = null;
                return;
            }

            List<LatheManufactureOrder> ordersInGroup = Orders
                .Where(x => 
                    x.State == OrderState.Complete && 
                    x.StartDate.Date > DateTime.MinValue && 
                    x.CreatedAt < SelectedRequest.DateRaised && 
                    x.GroupId == group.Id
                ).ToList();

            int countBefore = ordersInGroup.Count;
            int countBeforeLastYear = ordersInGroup.Where(x => x.StartDate.AddYears(1) > SelectedRequest.DateRaised).Count();

            List<LatheManufactureOrder> exactOrdersInYear = ordersInGroup.Where(x => x.StartDate.AddYears(1) > SelectedRequest.DateRaised && x.MaterialId == SelectedRequestProduct.MaterialId).ToList();
            int countBeforeLastYearInMaterial = exactOrdersInYear.Count;
            DateTime? lastRun = exactOrdersInYear.Count > 0 ? exactOrdersInYear.Max(x => x.StartDate) : null;

            FrequencyAnalysis = $"Run {countBefore:0} time(s) before." +
                $"{Environment.NewLine}{countBeforeLastYear:0} time(s) in the last year." +
                $"{Environment.NewLine}{countBeforeLastYearInMaterial:0} times in the last year in this material." +
                $"{Environment.NewLine}Last run in this material in {(lastRun is not null ? ((DateTime)lastRun).ToString("MMMM yyyy") : "n/a")}";

            if (exactOrdersInYear.Count == 0) return;


            List<LatheManufactureOrderItem> itemsMadeInYear = OrderItems
                .Where(x => exactOrdersInYear.Any(i => i.Name == x.AssignedMO) && x.QuantityDelivered > 0)
                .ToList();

            Dictionary<TurnedProduct, (int, int)> made = new();

            foreach (LatheManufactureOrderItem item in itemsMadeInYear)
            {
                TurnedProduct? p = Products.Find(x => x.ProductName == item.ProductName);

                if (p is null) continue;

                if (!made.ContainsKey(p))
                {
                    made.Add(p, (0, 0));
                }

                (int, int) val = made[p];
                made[p] = (val.Item1 + item.QuantityDelivered, val.Item2 + 1);
            }

            foreach (KeyValuePair<TurnedProduct, (int, int)> value in made)
            {
                TurnedProduct product = value.Key;
                int quantityShipped = value.Value.Item1;
                int frequencyShipped = value.Value.Item2;

                if(product.QuantitySold > quantityShipped)
                {
                    continue;
                }
                int delta = quantityShipped - product.QuantityInStock;

                //if (frequencyShipped == 1) continue;

                FrequencyAnalysis += $"{Environment.NewLine}{product.ProductName}: Target is {product.QuantitySold}; curr stock is {product.QuantityInStock}; shipped {quantityShipped}; {frequencyShipped} times; delta {delta:+#;-#;0}";
                if (product.QuantitySold == 0 && delta > 100)
                {
                    FrequencyAnalysis += $"{Environment.NewLine}\t{product.ProductName}: No target set; {delta:#,##0} sold.";
                    continue;
                }
                else if (product.QuantitySold == 0)
                {
                    FrequencyAnalysis += $"{Environment.NewLine}\tTarget is in excess of demand (zero sold)";
                    continue;
                }

                double ratio = (double)delta / (double)product.QuantitySold - 1;
                //if (delta < 0)
                //{
                //    continue;
                //}


                if (ratio < -0.06)
                {
                    FrequencyAnalysis += $"{Environment.NewLine}\tTarget is in excess of demand: {ratio:P2}";
                }
                else if (ratio > 0.06)
                {
                    FrequencyAnalysis += $"{Environment.NewLine}\tDemand is {ratio:P2} higher than target";
                }
                else
                {
                    FrequencyAnalysis += $"{Environment.NewLine}\tTarget is appropriate: {ratio:P2}";
                }
                
            }
        }


        private void LoadRecommendedOrder()
        {
            RecommendedManifest.Clear();

            if (SelectedRequest is null)
            {
                OnPropertyChanged(nameof(RecommendedManifest));
                return;
            }

            TimeSpan targetTimeSpan = TimeSpan.FromDays(Math.Round(TargetRuntime));
            // TODO add try catch
            RecommendedManifest = RequestsEngine.GetRecommendedOrderItems(Products, SelectedRequest, targetTimeSpan);
            OnPropertyChanged(nameof(RecommendedManifest));
        }

        public void ExportRequestsToCSV()
        {
            CSVHelper.WriteListToCSV(FilteredRequests.ToList(), "Lighthouse_Requests");
        }

        public void UpdateRequest()
        {
            if (SelectedRequest is null) return;

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

        public void UpdateRequirements(string notes, int QuantityRequired)
        {
            if(SelectedRequest is null) return;

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
            if (SelectedRequestProduct is null)
            {
                MessageBox.Show("Could not find product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedRequestProduct.GroupId is null || SelectedRequestProduct.MaterialId is null)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                OrderConstructorWindow creationWindow = new((int)SelectedRequestProduct.GroupId, (int)SelectedRequestProduct.MaterialId, RecommendedManifest) 
                {
                    Owner = Application.Current.MainWindow
                };
                creationWindow.SetConfirmButtonsVisibility(Visibility.Collapsed);
                creationWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public void ApproveRequest(bool merge = false)
        {
            if (SelectedRequest is null || SelectedRequestProduct is null)
            {
                return;
            }

            if (SelectedRequestProduct.HasErrors)
            {
                MessageBox.Show("The product record contains errors that must be addressed prior to approval.", "Data error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO Implement merging from requests
            if (!merge)
            {
                try
                {
                    OrderConstructorWindow creationWindow = new((int)SelectedRequestProduct.GroupId!, (int)SelectedRequestProduct.MaterialId!, RecommendedManifest)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    creationWindow.ShowDialog();

                    if (!creationWindow.SaveExit)
                    {
                        return;
                    }
                    SelectedRequest.ResultingLMO = creationWindow.NewOrder.Name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Order creation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Merging not implemented yet", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedRequest.MarkAsAccepted();

            //TODO Move to notifications manager
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

            if (!DatabaseHelper.Update(SelectedRequest))
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
            if (SelectedRequest is null) return;

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
                SelectedRequest = FilteredRequests.First();
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
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            MainProducts = DatabaseHelper.Read<Product>();
            Materials = DatabaseHelper.Read<MaterialInfo>();

            TargetRuntime = 5;

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();
            for (int i = 0; i < Orders.Count; i++)
            {
                Orders[i].OrderItems = OrderItems.Where(x => x.AssignedMO == Orders[i].Name).ToList();
            }

            List<Request> requests = DatabaseHelper.Read<Request>();
            for (int i = 0; i < requests.Count; i++)
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
                        (x.SubsequentOrder.State < OrderState.Complete && !string.IsNullOrEmpty(x.ResultingLMO)) ||
                        x.LastModified.AddDays(4) > DateTime.Now)
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

            OnPropertyChanged(nameof(FilteredRequests));
            OnPropertyChanged(nameof(SelectedFilter));
        }

        public void SendMessage()
        {
            if (SelectedRequest is null) return;

            Note newNote = new()
            {
                Message = NewMessage,
                OriginalMessage = NewMessage,
                DateSent = DateTime.Now.ToString("s"),
                DocumentReference = $"r{SelectedRequest.Id:0}",
                SentBy = App.CurrentUser.UserName,
            };

            try
            {
                DatabaseHelper.Insert(newNote, throwErrs: true);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            otherUsers.AddRange(App.NotificationsManager.users.Where(x => x.HasPermission(PermissionType.ApproveRequest)).Select(x => x.UserName));
            otherUsers.Add(App.NotificationsManager.users.Find(x => x.GetFullName() == SelectedRequest.RaisedBy)!.UserName);

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i], 
                    App.CurrentUser.UserName, 
                    $"Comment: Request #{SelectedRequest.Id:0}", 
                    $"{App.CurrentUser.FirstName} left a comment: {NewMessage}", toastAction:$"viewRequest:{SelectedRequest.Id:0}");
                _ = DatabaseHelper.Insert(newNotification);
            }

            Notes.Add(newNote);
            LoadRequestCard();

            NewMessage = "";
        }
    }
}
