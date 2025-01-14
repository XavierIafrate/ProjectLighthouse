using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.View.Administration;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.View.Requests;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Commands.Administration;
using ProjectLighthouse.ViewModel.Commands.Requests;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.ValueConverters;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Requests
{
    public class RequestViewModel : BaseViewModel
    {
        #region Variables
        public List<Request> Requests { get; set; }
        public List<RequestItem> RequestItems { get; set; }

        private List<Request> filteredRequests;
        public List<Request> FilteredRequests
        {
            get { return filteredRequests; }
            set
            {
                filteredRequests = value;
                OnPropertyChanged();
            }
        }

        public List<Product> Products { get; set; }
        public List<ProductGroup> Archetypes { get; set; }
        public List<TurnedProduct> Items { get; set; }

        public List<LatheManufactureOrder> Orders { get; set; }
        public List<LatheManufactureOrderItem> OrderItems { get; set; }
        public List<LatheManufactureOrderItem> RecommendedManifest { get; set; }

        private List<LatheManufactureOrder> activeOrders;

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

        private string? searchString;
        public string? SearchString
        {
            get { return searchString; }
            set
            {
                if (value is not null)
                {
                    if (value == "")
                    {
                        value = null;
                    }
                }
                searchString = value;
                Search();
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

                LoadRecommendedOrder();
            }
        }

        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                selectedFilter = value;
                Search();
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

        private List<RequestItem> selectedRequestItems;
        public List<RequestItem> SelectedRequestItems
        {
            get { return selectedRequestItems; }
            set
            {
                if (selectedRequestItems != null)
                {
                    foreach (RequestItem item in selectedRequestItems)
                    {
                        item.ClearSubscribers();
                    }
                }

                selectedRequestItems = value;
                OnPropertyChanged();
            }
        }

        private Product? selectedRequestProduct;
        public Product? SelectedRequestProduct
        {
            get { return selectedRequestProduct; }
            set
            {
                selectedRequestProduct = value;
                OnPropertyChanged();
            }
        }

        private ProductGroup? selectedRequestArchetype;
        public ProductGroup? SelectedRequestArchetype
        {
            get { return selectedRequestArchetype; }
            set
            {
                selectedRequestArchetype = value;
                OnPropertyChanged();
            }
        }

        private LatheManufactureOrder? selectedRequestOrder;

        public LatheManufactureOrder? SelectedRequestOrder
        {
            get { return selectedRequestOrder; }
            set { selectedRequestOrder = value; OnPropertyChanged(); }
        }

        private DateTime? lastSyncRun;

        public DateTime? LastSyncRun
        {
            get { return lastSyncRun; }
            set 
            { 
                lastSyncRun = value; 
                OnPropertyChanged(); 
                if(value is not null)
                {
                    SyncHasNotRunRecently = (DateTime.Now - (DateTime)value).TotalDays > 3;
                }
                else
                {
                    SyncHasNotRunRecently = true;
                }
            }
        }

        private bool syncHasNotRunRecently = true;

        public bool SyncHasNotRunRecently
        {
            get { return syncHasNotRunRecently; }
            set { syncHasNotRunRecently = value; OnPropertyChanged(); }
        }





        #region Visibilities

        private bool showRecommendation;
        public bool ShowRecommendation
        {
            get { return showRecommendation; }
            set
            {
                showRecommendation = value;
                OnPropertyChanged();
            }
        }

        private bool showApproval;
        public bool ShowApproval
        {
            get { return showApproval; }
            set
            {
                showApproval = value;
                OnPropertyChanged();
            }
        }

        private bool changesMade;
        public bool ChangesMade
        {
            get { return changesMade; }
            set
            {
                changesMade = value;
                OnPropertyChanged();
            }
        }

        private bool newRequestMode;
        public bool NewRequestMode
        {
            get { return newRequestMode; }
            set
            {
                newRequestMode = value;
                OnPropertyChanged();
            }
        }

        private Request? newRequest;

        public Request? NewRequest
        {
            get { return newRequest; }
            set { newRequest = value; OnPropertyChanged(); }
        }

        private List<RequestItem> newRequestItems;

        public List<RequestItem> NewRequestItems
        {
            get { return newRequestItems; }
            set { newRequestItems = value; OnPropertyChanged(); }
        }

        private string? newRequestSearchText;

        public string? NewRequestSearchText
        {
            get { return newRequestSearchText; }
            set
            {
                newRequestSearchText = value;
                OnPropertyChanged();
                SearchItems();
            }
        }

        private bool canEditRequestItems = true;

        public bool CanEditRequestItems
        {
            get { return canEditRequestItems; }
            set { canEditRequestItems = value; OnPropertyChanged(); }
        }

        private List<TurnedProduct> newRequestSearchResults;

        public List<TurnedProduct> NewRequestSearchResults
        {
            get { return newRequestSearchResults; }
            set { newRequestSearchResults = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ApproveRequestCommand ApproveCommand { get; set; }
        public DeclineRequestCommand DeclineCommand { get; set; }
        public ViewMakeOrBuyCommand ViewMakeOrBuyCommand { get; set; }
        public UpdateRequestCommand UpdateRequestCmd { get; set; }
        public EditItemCommand EditItemCmd { get; set; }
        public SendMessageCommand SendMessageCommand { get; set; }
        public NewRequestCommand NewRequestCmd { get; set; }
        public SubmitRequestCommand SubmitRequestCmd { get; set; }
        public AddToRequestCommand AddToRequestCmd { get; set; }
        public RemoveFromRequestCommand RemoveFromRequestCmd { get; set; }
        public NewSpecialPartCommand AddSpecialCmd { get; set; }

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
            ViewMakeOrBuyCommand = new(this);
            EditItemCmd = new(this);
            UpdateRequestCmd = new(this);
            SendMessageCommand = new(this);
            NewRequestCmd = new(this);
            SubmitRequestCmd = new(this);
            AddToRequestCmd = new(this);
            AddSpecialCmd = new(this);

            Notes = new();
            TargetRuntime = 5;
        }

        public void LoadData()
        {
            Notes = DatabaseHelper.Read<Note>().ToList();
            Products = DatabaseHelper.Read<Product>();
            Archetypes = DatabaseHelper.Read<ProductGroup>();
            Items = DatabaseHelper.Read<TurnedProduct>().OrderBy(x => x.ProductName).ToList();
            RequestItems = DatabaseHelper.Read<RequestItem>();
            TargetRuntime = 5;

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();
            OrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>();

            ApplicationVariable? lastSyncTime = DatabaseHelper.Read<ApplicationVariable>().Find(x => x.Id == "SYNC_LAST_RUN");

            if(lastSyncTime is not null)
            {
                try
                {
                    DateTime lastSyncTimeVal = (DateTime)lastSyncTime.Data;
                    LastSyncRun = lastSyncTimeVal;
                }
                catch
                {
                    // do nothing
                }
            }


            for (int i = 0; i < Orders.Count; i++)
            {
                Orders[i].OrderItems = OrderItems.Where(x => x.AssignedMO == Orders[i].Name).ToList();
            }

            activeOrders = Orders.Where(x => x.State < OrderState.Complete).ToList();

            List<Request> requests;
            try
            {
                requests = DatabaseHelper.Read<Request>(throwErrs: true)
                    .OrderByDescending(x => x.RaisedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Requests = requests;

            SelectedFilter = "Last 14 Days";
        }

        public void RaiseRequest()
        {
            if (NewRequest is not null)
            {
                SelectedRequest = FilteredRequests.Find(x => x == NewRequest);
                SelectedRequest ??= FilteredRequests.First();
                return;
            }

            NewRequest = new() 
            { 
                Id = 0, 
                RaisedAt = DateTime.Now, 
                RaisedBy = App.CurrentUser.GetFullName(), 
                Description = "New Request", 
                Status = Request.RequestStatus.Draft 
            };

            NewRequestItems = new();

            FilteredRequests = FilteredRequests.Prepend(NewRequest).ToList();
            SelectedRequest = FilteredRequests.First();
        }

        private void SearchItems()
        {
            int[] requestedIds = NewRequestItems.Select(x => x.ItemId).ToArray();

            if (NewRequestItems.Count > 0)
            {
                TurnedProduct? p = NewRequestItems.First().Item ?? throw new Exception("Could not find item"); // ??
                SelectedRequestArchetype = Archetypes.Find(x => x.Id == p.GroupId);
                if (SelectedRequestArchetype is not null)
                {
                    SelectedRequestProduct = Products.Find(x => x.Id == SelectedRequestArchetype.ProductId);
                }

                if (p.GroupId is not null)
                {
                    NewRequestSearchResults = Items.Where(x => x.GroupId == p.GroupId && x.MaterialId == p.MaterialId & !requestedIds.Contains(x.Id)).ToList();
                }
                else
                {
                    NewRequestSearchResults = new();
                }

                PopulateInsightsFields();

                return;
            }


            if (string.IsNullOrWhiteSpace(NewRequestSearchText))
            {
                List<TurnedProduct> activeReqs = Items
                    .Where(x => x.FreeStock() < 0)
                    .OrderBy(n => n.ProductName)
                    .ToList();

                AddActiveOrderInformation(activeReqs);

                activeReqs = activeReqs
                    .Where(x => x.FreeStock() < 0)
                    .ToList();

                List<TurnedProduct> urgent = Items
                    .Where(x => x.QuantityInStock - x.QuantitySold < -500 && (double)x.QuantityInStock / x.QuantitySold < 0.1 && activeOrders.Find(o => o.GroupId == x.GroupId && o.MaterialId == x.MaterialId) == null)
                    .OrderBy(n => n.ProductName)
                    .ToList();

                activeReqs.AddRange(urgent);

                activeReqs = activeReqs.Distinct().ToList();

                NewRequestSearchResults = activeReqs;
            }
            else
            {
                string searchTerm = NewRequestSearchText.ToUpperInvariant();
                List<TurnedProduct> results = Items.Where(x => x.ProductName.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) && !requestedIds.Contains(x.Id)).Take(50).ToList();
                
                foreach(TurnedProduct result in results)
                {
                    if (result.GroupId is null) continue;

                    ProductGroup? archetype = Archetypes.Find(x => x.Id == result.GroupId);
                    if (archetype is null) continue;
                    if (archetype.Status == ProductGroup.GroupStatus.Dormant)
                    {
                        result.Retired = true;
                    }
                }
                
                NewRequestSearchResults = results;
            }

            PopulateInsightsFields();
        }

        private void PopulateInsightsFields()
        {
            AddActiveOrderInformation(NewRequestSearchResults);
        }

        private void AddActiveOrderInformation(List<TurnedProduct> items)
        {
            foreach (TurnedProduct item in items)
            {
                item.AppendableOrder = activeOrders.Find(x => x.GroupId == item.GroupId && x.MaterialId == item.MaterialId);
                item.LighthouseGuaranteedQuantity = 0;

                if (item.AppendableOrder is not null)
                {
                    LatheManufactureOrderItem itemOnActiveOrder = item.AppendableOrder.OrderItems.Find(x => x.ProductName == item.ProductName);
                    if (itemOnActiveOrder is not null)
                    {
                        item.LighthouseGuaranteedQuantity += itemOnActiveOrder.RequiredQuantity;
                    }
                }
                else
                {
                    item.ZeroSetOrder = activeOrders.Find(x => x.GroupId == item.GroupId);
                }
            }
        }

        public void AddToRequest(TurnedProduct item)
        {
            if (NewRequest is null) return;
            List<RequestItem> updatedList = NewRequestItems;
            RequestItem newItem = new()
            {
                QuantityRequired = 0,
                ItemId = item.Id,
                Item = Items.Find(x => x.Id == item.Id),
                DateRequired = null,
                CreatedAt = DateTime.Now,
                CreatedBy = App.CurrentUser.UserName
            };

            if (newItem.Item is null)
            {
                MessageBox.Show("Could not find item to add", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (item.FreeStock() < 0)
            {
                newItem.QuantityRequired = item.FreeStock() * -1;
            }

            updatedList.Add(newItem);

            if (updatedList.Count == 1 || SelectedRequestArchetype is null)
            {
                NewRequest.Description = newItem.Item.ProductName;
            }
            else if (SelectedRequestArchetype is not null)
            {
                NewRequest.Description = $"{SelectedRequestArchetype.Name} [{updatedList.Count}]";
            }
            else
            {
                NewRequest.Description = "New Request";
            }

            NewRequestItems = updatedList;
            SelectedRequestItems = null;
            SelectedRequestItems = NewRequestItems;
            SearchItems();
        }

        public void RemoveFromRequest(RequestItem item)
        {
            if (SelectedRequest == null) return;

            if (SelectedRequest == NewRequest)
            {
                RemoveFromDraft(item);
            }
            else
            {
                RemoveFromActive(item);
            }
        }

        private void RemoveFromDraft(RequestItem item)
        {
            if (NewRequest == null) return;

            List<RequestItem> updatedList = NewRequestItems;
            RequestItem? removedItem = NewRequestItems.Find(x => x.ItemId == item.ItemId);

            if (removedItem is null)
            {
                MessageBox.Show("Could not find item");
                return;
            }

            updatedList.Remove(removedItem);
            if (updatedList.Count == 0)
            {
                NewRequest.Description = "New Request";
            }
            else if (updatedList.Count == 1 || SelectedRequestArchetype is null)
            {
                NewRequest.Description = updatedList.First().Item!.ProductName;
            }
            else if (SelectedRequestArchetype is not null)
            {
                NewRequest.Description = $"{SelectedRequestArchetype.Name} [{updatedList.Count}]";
            }
            else
            {
                NewRequest.Description = "New Request";
            }

            NewRequestItems = updatedList;
            SelectedRequestItems = null;
            SelectedRequestItems = NewRequestItems;
            SearchItems();
        }

        private void RemoveFromActive(RequestItem item)
        {
            if (SelectedRequest is null) return;
            List<RequestItem> updatedList = SelectedRequestItems;

            if (SelectedRequestItems.Count == 1)
            {
                MessageBox.Show("Cannot remove item as request will be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            RequestItem? removedItem = SelectedRequestItems.Find(x => x.ItemId == item.ItemId);

            if (removedItem is null)
            {
                MessageBox.Show("Could not find item");
                return;
            }

            updatedList.Remove(removedItem);

            if (updatedList.Count == 1 || SelectedRequestArchetype is null)
            {
                SelectedRequest.Description = updatedList.First().Item!.ProductName;
            }
            else if (SelectedRequestArchetype is not null)
            {
                SelectedRequest.Description = $"{SelectedRequestArchetype.Name} [{updatedList.Count}]";
            }
            else
            {
                SelectedRequest.Description = "New Request";
            }

            SelectedRequestItems = null;
            SelectedRequestItems = NewRequestItems;
            SearchItems();
        }

        public void SubmitRequest()
        {
            if (NewRequestItems.Count == 0) return;
            if (NewRequestItems.Any(x => x.Item == null)) return;
            if (NewRequest == null) return;

            for (int i = 0; i < NewRequestItems.Count; i++)
            {
                NewRequestItems[i].ValidateAll();
            }

            if (NewRequestItems.Any(i => i.HasErrors))
            {
                return;
            }

            try
            {
                NewRequest.Status = Request.RequestStatus.Pending;
                NewRequest.TotalQuantity = NewRequestItems.Sum(x => x.QuantityRequired);
                NewRequest.RaisedAt = DateTime.Now;
                NewRequest.ArchetypeId = NewRequestItems.First().Item!.GroupId;
                DatabaseHelper.Insert(NewRequest, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            for (int i = 0; i < NewRequestItems.Count; i++)
            {
                NewRequestItems[i].RequestId = NewRequest.Id;
                NewRequestItems[i].CreatedAt = NewRequest.RaisedAt;
                DatabaseHelper.Insert(NewRequestItems[i]);
            }


            Requests.Add(NewRequest);
            Requests = Requests.OrderByDescending(x => x.RaisedAt).ToList();
            RequestItems.AddRange(NewRequestItems);

            App.NotificationsManager.NotifyRequestRaised(NewRequest);
            newRequest = null;
            newRequestItems = new();

            Search();
        }

        public void EditItem(RequestItem requestItem)
        {
            AddTurnedProductWindow window = new(Archetypes, requestItem.Item)
            {
                Owner = App.MainViewModel.MainWindow
            };

            window.ShowDialog();
            if (!window.SaveExit)
            {
                return;
            }

            Items = DatabaseHelper.Read<TurnedProduct>();

            LoadRequestCard();
        }

        public void CreateSpecial()
        {
            RaiseSpecialRequest window = new()
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            if (window.productAdded)
            {
                Items.Add(window.NewProduct);
                AddToRequest(window.NewProduct);
            }
        }

        public void LoadRequestCard()
        {
            if (SelectedRequest is null) return;

            NewRequestMode = false;
            ChangesMade = false;

            SelectedRequestOrder = Orders.Find(x => x.Id == SelectedRequest.OrderId);

            SelectedRequestOrder?.OrderItems.ForEach(x =>
                {
                    TurnedProduct? p = Items.Find(i => i.ProductName == x.ProductName);
                    if (p != null)
                    {
                        x.Gtin = p.ExportProductName;
                    }
                });

            if (SelectedRequest == NewRequest)
            {
                RemoveFromRequestCmd = new(this);

                SelectedRequestProduct = null;
                SelectedRequestArchetype = null;
                NewRequestSearchText = null;
                SelectedRequestItems = NewRequestItems;
                ShowRecommendation = false;
                NewRequestMode = true;
                CanEditRequestItems = true;
                ShowApproval = false;
                App.MainViewModel.NavText = App.CurrentUser.Locale switch
                {
                    "Polish" => "Nowe Żądanie",
                    "Persian" => "درخواست جدید",
                    "Welsh" => "Cais Newydd",
                    "Latvian" => "Jauns Pieprasījums",
                    _ => "New Request"
                };
                return;
            }

            RemoveFromRequestCmd = null;
            CanEditRequestItems = App.CurrentUser.GetFullName() == SelectedRequest.RaisedBy && SelectedRequest.Status == Request.RequestStatus.Pending;

            App.MainViewModel.NavText = App.CurrentUser.Locale switch
            {
                "Polish" => "Wyświetl Prośby",
                "Persian" => "مشاهده درخواست ها",
                "Welsh" => "Gweld Ceisiadau",
                "Latvian" => "Pieprasījumi",
                _ => "Requests"
            };

            ShowApproval = SelectedRequest.Status == Request.RequestStatus.Pending;
            ShowRecommendation = SelectedRequest.Status == Request.RequestStatus.Pending;


            if (SelectedRequest.ArchetypeId is not null)
            {
                SelectedRequestArchetype = Archetypes.Find(x => x.Id == SelectedRequest.ArchetypeId);
                if (SelectedRequestArchetype != null)
                {
                    SelectedRequestProduct = Products.Find(x => x.Id == SelectedRequestArchetype.ProductId);
                }
            }
            else
            {
                SelectedRequestArchetype = null;
                SelectedRequestProduct = null;
            }

            // I hate that this works
            string breakReferences = Newtonsoft.Json.JsonConvert.SerializeObject(RequestItems.Where(x => x.RequestId == SelectedRequest.Id).ToList());
            SelectedRequestItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RequestItem>>(breakReferences);

            foreach (RequestItem requestItem in SelectedRequestItems!)
            {
                requestItem.Item = Items.Find(x => x.Id == requestItem.ItemId);
                requestItem.Item?.ValidateForOrder(); // TODO check
                requestItem.RequirementChanged += EvaluateCanSaveChanges;
            }

            if (SelectedRequestArchetype == null && SelectedRequestItems[0].Item?.GroupId != null)
            {
                SelectedRequest.ArchetypeId = SelectedRequestItems[0].Item?.GroupId;
                DatabaseHelper.Update(SelectedRequest);
                SelectedRequestArchetype = Archetypes.Find(x => x.Id == SelectedRequest.ArchetypeId);
            }

            FilteredNotes = null;
            OnPropertyChanged(nameof(FilteredNotes));
            FilteredNotes = Notes.Where(x => x.DocumentReference == $"r{SelectedRequest.Id:0}").ToList();
            OnPropertyChanged(nameof(FilteredNotes));
            if (ShowRecommendation)
            {
                LoadRecommendedOrder();
            }
        }

        private void EvaluateCanSaveChanges()
        {
            for (int i = 0; i < SelectedRequestItems.Count; i++)
            {
                RequestItem item = SelectedRequestItems[i];
                RequestItem? cachedItem = RequestItems.Find(x => x.Id == item.Id);
                if (cachedItem == null)
                {
                    ChangesMade = true;
                    return;
                }

                if (item.DateRequired != cachedItem.DateRequired || item.QuantityRequired != cachedItem.QuantityRequired)
                {
                    ChangesMade = true;
                    return;
                }
            }

            if (SelectedRequestItems.Count != RequestItems.Where(x => x.RequestId == SelectedRequest!.Id).Count())
            {
                ChangesMade = true;
                return;
            }

            ChangesMade = false;

        }

        void LoadRecommendedOrder()
        {
            if (Items == null || SelectedRequestItems == null || SelectedRequestArchetype == null)
            {
                RecommendedManifest = new();
                OnPropertyChanged(nameof(RecommendedManifest));
                if (RecommendedManifest.Count == 0)
                {
                    ShowRecommendation = false;
                }
                return;
            }

            try
            {
                RecommendedManifest = RequestsEngine.GetRecommendedOrderItems(Items, SelectedRequestItems, TimeSpan.FromDays(TargetRuntime));
            }
            catch
            {
                RecommendedManifest = new();
            }
            OnPropertyChanged(nameof(RecommendedManifest));
            if (RecommendedManifest.Count == 0)
            {
                ShowRecommendation = false;
            }
        }

        public void UpdateRequest()
        {
            if (SelectedRequest is null) return;

            for (int i = 0; i < SelectedRequestItems.Count; i++)
            {
                RequestItem item = SelectedRequestItems[i];
                RequestItem? cachedItem = RequestItems.Find(x => x.Id == item.Id);
                if (cachedItem == null)
                {
                    MessageBox.Show("Could not find item", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (item.DateRequired != cachedItem.DateRequired || item.QuantityRequired != cachedItem.QuantityRequired)
                {
                    try
                    {
                        DatabaseHelper.Update(item, true);
                    }
                    catch (Exception ex)
                    {
                        NotificationManager.NotifyHandledException(ex);
                    }
                }
            }

            RequestItems = DatabaseHelper.Read<RequestItem>();

            SelectedRequest.ModifiedAt = DateTime.Now;
            SelectedRequest.ModifiedBy = App.CurrentUser.GetFullName();
            SelectedRequest.TotalQuantity = SelectedRequestItems.Sum(x => x.QuantityRequired);

            if (DatabaseHelper.Update(SelectedRequest))
            {
                OnPropertyChanged(nameof(SelectedRequest));
            }
            else
            {
                MessageBox.Show("Failed to update request", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            EvaluateCanSaveChanges();
            LoadRecommendedOrder();
        }

        public void ShowMakeOrBuy()
        {
            if (SelectedRequestItems.Any(x => x.Item is null))
            {
                MessageBox.Show("Could not find product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedRequest is null)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedRequestArchetype is null)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedRequest.ArchetypeId is null)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int materialId = SelectedRequestItems.First().Item!.MaterialId ?? -1;

            if (materialId == -1)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                OrderConstructorWindow creationWindow = new((int)SelectedRequest.ArchetypeId, materialId, RecommendedManifest)
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

        public void ApproveRequest()
        {
            if (SelectedRequest is null || SelectedRequestArchetype == null)
            {
                return;
            }

            if (SelectedRequestItems.Any(x => x.HasErrors))
            {
                MessageBox.Show("A product record contains errors that must be addressed prior to approval.", "Data error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedRequestItems.Any(x => x.Item == null))
            {
                MessageBox.Show("An item could not be found.", "Data error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int materialId = SelectedRequestItems.First().Item!.MaterialId ?? -1;

            if (materialId == -1)
            {
                MessageBox.Show("Required information is missing", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

           
            try
            {
                OrderConstructorWindow creationWindow = new((int)SelectedRequest.ArchetypeId!, materialId, RecommendedManifest)
                {
                    Owner = Application.Current.MainWindow
                };
                creationWindow.ShowDialog();

                if (!creationWindow.SaveExit)
                {
                    return;
                }

                SelectedRequest.OrderId = creationWindow.NewOrder.Id;
                SelectedRequest.order = creationWindow.NewOrder;
                Orders.Add(creationWindow.NewOrder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Order creation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SQLiteConnection con = DatabaseHelper.GetConnection();
            con.BeginTransaction();

            SelectedRequest.Mark(accepted: true);

            if (con.Update(SelectedRequest) != 1)
            {
                con.Rollback();
                con.Close();
                MessageBox.Show("Failed to update request", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            con.Commit();
            con.Close();

            int id = SelectedRequest.Id;

            App.NotificationsManager.NotifyRequestApproved(SelectedRequest);

            new ToastContentBuilder()
                .AddText($"Order {SelectedRequest.order.Name} created.")
                .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                .AddText("You have successfully approved this request.")
                .Show();

            Search();

            SelectedRequest = FilteredRequests.Find(x => x.Id == id);
        }

        public void DeclineRequest()
        {
            if (SelectedRequest is null) return;

            if (!App.CurrentUser.HasPermission(PermissionType.ApproveRequest))
            {
                MessageBox.Show("You do not have permission to decline requests.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            SelectedRequest.Mark(accepted: false);

            try
            {
                DatabaseHelper.Update(SelectedRequest, throwErrs: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the request:{Environment.NewLine}{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            int id = SelectedRequest.Id;

            Search();

            SelectedRequest = FilteredRequests.Find(x => x.Id == id);
            if (SelectedRequest is null && FilteredRequests.Count > 0)
            {
                SelectedRequest = FilteredRequests[0];
            }
        }

        public void Search()
        {
            NewRequest = null;

            if (string.IsNullOrEmpty(SearchString))
            {
                switch (SelectedFilter)
                {
                    case "All":
                        FilteredRequests = Requests;
                        break;
                    case "Last 14 Days":
                        FilteredRequests = Requests.Where(n => n.RaisedAt.AddDays(14) > DateTime.Now).ToList();
                        break;
                    case "Pending":
                        FilteredRequests = Requests.Where(n => n.Status == Request.RequestStatus.Pending).ToList();
                        break;
                    case "Accepted":
                        FilteredRequests = Requests.Where(n => n.Status == Request.RequestStatus.Accepted).ToList();
                        break;
                    case "Declined":
                        FilteredRequests = Requests.Where(n => n.Status == Request.RequestStatus.Declined).ToList();
                        break;
                    case "My Requests":
                        FilteredRequests = Requests.Where(n => n.RaisedBy == App.CurrentUser.UserName).ToList();
                        break;
                    default:
                        break;
                }
                SelectFirstRequestIfPossible();
                return;
            }

            string searchToken = SearchString.Trim().ToUpperInvariant();
            List<int> requestIds = new();

            requestIds.AddRange(RequestItems.Where(x => x.Item is not null).Where(x => x.Item!.ProductName.Contains(searchToken)).Select(x => x.RequestId).ToList());
            requestIds.AddRange(Requests.Where(x => x.Description.Contains(searchToken)).Select(x => x.Id).ToList());

            requestIds = requestIds.Distinct().ToList();

            FilteredRequests = Requests.Where(x => requestIds.Contains(x.Id)).OrderByDescending(x => x.RaisedAt).ToList();

            SelectFirstRequestIfPossible();
        }

        void SelectFirstRequestIfPossible()
        {
            if (FilteredRequests.Count > 0)
            {
                SelectedRequest = FilteredRequests.First();
            }
            else
            {
                SelectedRequest = null;
            }
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
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while inserting to the database:{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> otherUsers = FilteredNotes.Select(x => x.SentBy).ToList();
            otherUsers.AddRange(App.NotificationsManager.Users.Where(x => x.HasPermission(PermissionType.ApproveRequest)).Select(x => x.UserName));

            User? userWhoRaisedRequest = App.NotificationsManager.Users.Find(x => x.GetFullName() == SelectedRequest.RaisedBy);
            if (userWhoRaisedRequest is not null)
            {
                otherUsers.Add(userWhoRaisedRequest.UserName);
            }

            otherUsers = otherUsers.Where(x => x != App.CurrentUser.UserName).Distinct().ToList();

            for (int i = 0; i < otherUsers.Count; i++)
            {
                Notification newNotification = new(otherUsers[i],
                    App.CurrentUser.UserName,
                    $"Comment: Request #{SelectedRequest.Id:0}",
                    $"{App.CurrentUser.FirstName} left a comment: {NewMessage}", toastAction: $"viewRequest:{SelectedRequest.Id:0}");
                _ = DatabaseHelper.Insert(newNotification);
            }

            Notes.Add(newNote);
            LoadRequestCard();

            NewMessage = "";
        }
    }
}
