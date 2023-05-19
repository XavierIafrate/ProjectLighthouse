using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.HelperWindows;
using ProjectLighthouse.View.Requests;
using ProjectLighthouse.ViewModel.Commands.Requests;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Requests
{
    public class NewRequestViewModel : BaseViewModel
    {
        #region Variables
        private Request newRequest;
        public Request NewRequest
        {
            get { return newRequest; }
            set
            {
                newRequest = value;
                OnPropertyChanged();
            }
        }

        private string searchTerm;
        public string SearchTerm
        {
            get { return searchTerm; }
            set
            {
                searchTerm = value.Trim().ToUpper();
                FilterProducts(searchTerm);
                OnPropertyChanged();
            }
        }

        public bool ShowingLiveRequirements { get; set; }
        public bool NoSearchResults { get; set; }

        public List<TurnedProduct> TurnedProducts { get; set; }
        private List<TurnedProduct> filteredList;

        public List<TurnedProduct> FilteredList
        {
            get { return filteredList; }
            set
            {
                filteredList = value;
                if (filteredList.Count > 0)
                {
                    SelectedProduct = filteredList[0];
                }
                else
                {
                    SelectedProduct = null;
                }
                OnPropertyChanged();
            }
        }


        public List<TurnedProduct> ItemsOnOrder;
        public List<Request> RecentlyDeclinedRequests;

        private TurnedProduct? selectedProduct;
        public TurnedProduct? SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                SetRequest();
                OnPropertyChanged();
            }
        }

        private ProductGroup? selectedProductGroup;
        public ProductGroup? SelectedProductGroup
        {
            get { return selectedProductGroup; }
            set 
            { 
                selectedProductGroup = value;
                OnPropertyChanged();
            }
        }

        private Product? selectedMainProduct;
        public Product? SelectedMainProduct
        {
            get { return selectedMainProduct; }
            set
            {
                selectedMainProduct = value;
                OnPropertyChanged();
            }
        }

        private MaterialInfo? selectedProductMaterial;

        public MaterialInfo? SelectedProductMaterial
        {
            get { return selectedProductMaterial; }
            set 
            { 
                selectedProductMaterial = value;
                OnPropertyChanged();    
            }
        }


        public List<LeadTime> Lathes { get; set; }

        public List<User> ToNotify { get; set; }

        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<MaterialInfo> Materials { get; set; }

        public List<LatheManufactureOrderItem> RecommendedManifest { get; set; }

        public NewSpecialPartCommand AddSpecialCommand { get; set; }
        public NewRequestCommand SubmitRequestCommand { get; set; }

        #endregion

        // TODO: Refactor
        public NewRequestViewModel()
        {
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .Where(x => !x.Retired)
                .OrderBy(x => x.ProductName).ToList();

            List<LatheManufactureOrder> ActiveOrders = DatabaseHelper.Read<LatheManufactureOrder>()
                .Where(x => x.State < OrderState.Complete)
                .ToList();
            string[] activeOrders = ActiveOrders.Select(x => x.Name).ToArray();

            List<LatheManufactureOrderItem> ActiveOrderItems = DatabaseHelper.Read<LatheManufactureOrderItem>()
                .Where(x => activeOrders.Contains(x.AssignedMO))
                .ToList();

            for (int i = 0; i < ActiveOrders.Count; i++)
            {
                ActiveOrders[i].OrderItems = ActiveOrderItems.Where(x => x.AssignedMO == ActiveOrders[i].Name).ToList();
            }

            List<Request> recentlyDeclinedRequests = DatabaseHelper.Read<Request>().Where(x => x.IsDeclined && x.LastModified.AddDays(14) > DateTime.Now).ToList();

            TurnedProducts = RequestsEngine.PopulateInsightFields(TurnedProducts, ActiveOrders, recentlyDeclinedRequests);

            Products = DatabaseHelper.Read<Product>();
            ProductGroups = DatabaseHelper.Read<ProductGroup>();
            Materials = DatabaseHelper.Read<MaterialInfo>();

            ToNotify = DatabaseHelper.Read<User>().Where(x => x.HasPermission(PermissionType.ApproveRequest) && x.ReceivesNotifications).ToList();

            NewRequest = new();
            SubmitRequestCommand = new(this);
            AddSpecialCommand = new(this);
            FilteredList = new();
            SelectedProduct = new();
            SearchTerm = "";
            GetLeadTimes();
        }

        private void GetLeadTimes()
        {
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().Where(x => !x.OutOfService).ToList();
            Lathes = new();

            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>().Where(x => x.State < OrderState.Complete).ToList();
            List<LatheManufactureOrderItem> items = DatabaseHelper.Read<LatheManufactureOrderItem>().ToList();

            List<MachineService> servicing = DatabaseHelper.Read<MachineService>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();
            List<ResearchTime> research = DatabaseHelper.Read<ResearchTime>().Where(x => x.StartDate.AddSeconds(x.TimeToComplete) > DateTime.Now).ToList();

            List<ScheduleItem> CompleteOrders = new();

            CompleteOrders.AddRange(research);
            CompleteOrders.AddRange(servicing);
            foreach (LatheManufactureOrder order in orders)
            {
                order.OrderItems = items.Where(i => i.AssignedMO == order.Name).ToList();
                CompleteOrders.Add(order);
            }

            for (int i = 0; i < lathes.Count; i++)
            {
                List<ScheduleItem> ordersForLathe = CompleteOrders.Where(o => o.AllocatedMachine == lathes[i].Id).ToList();
                TimeSpan workload = WorkloadCalculationHelper.GetMachineWorkload(ordersForLathe);

                LeadTime tmpSnippet = new()
                {
                    Lathe = lathes[i],
                    Workload = workload,
                };


                Lathes.Add(tmpSnippet);
            }

            OnPropertyChanged(nameof(Lathes));
        }

        private void SetRequest()
        {
            if (SelectedProduct == null)
            {
                NewRequest = null;
                SelectedMainProduct= null;
                SelectedProductGroup= null;
                RecommendedManifest = null;
                OnPropertyChanged(nameof(RecommendedManifest));
                return;
            }

            if (SelectedProduct.GroupId is not null)
            {
                SelectedProductGroup = ProductGroups.Find(x => x.Id == SelectedProduct.GroupId);
                if(SelectedProductGroup is not null)
                {
                    SelectedMainProduct = Products.Find(x => x.Id == SelectedProductGroup.ProductId);
                }
            }
            else
            {
                SelectedProductGroup = null;
                SelectedMainProduct = null;
            }

            if (SelectedProduct.MaterialId is not null)
            {
                SelectedProductMaterial = Materials.Find(x => x.Id == SelectedProduct.MaterialId);
            }
            else
            {
                SelectedProductMaterial = null;
            }

            NewRequest = new()
            {
                Product = SelectedProduct.ProductName,
                QuantityRequired = Math.Max(-SelectedProduct.FreeStock(), 0),
                DateRequired = DateTime.Now.AddMonths(1),
            };

            GetRecommendedManifest();
        }

        public void GetRecommendedManifest()
        {
            RecommendedManifest = null;
            OnPropertyChanged(nameof(RecommendedManifest));

            if (SelectedProduct == null || NewRequest == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(NewRequest.Product))
            {
                return;
            }

            try
            {
                RecommendedManifest = RequestsEngine.GetRecommendedOrderItems(TurnedProducts,
                                                                                NewRequest,
                                                                                TimeSpan.FromDays(5));
            }
            catch
            {
                RecommendedManifest = new();
            }

            OnPropertyChanged(nameof(RecommendedManifest));
        }

        private void FilterProducts(string searchTerm)
        {
            ShowingLiveRequirements = string.IsNullOrEmpty(searchTerm);
            OnPropertyChanged(nameof(ShowingLiveRequirements));
            if (string.IsNullOrEmpty(searchTerm))
            {
                FilteredList = TurnedProducts
                    .Where(x => x.FreeStock() < 0)
                    .OrderBy(n => n.MaterialId)
                    .ThenBy(n => n.ProductName)
                    .ToList();

                NoSearchResults = false;
            }
            else
            {
                FilteredList = TurnedProducts.Where(x => x.ProductName.ToUpper().Contains(searchTerm)).Take(100).ToList();
                NoSearchResults = FilteredList.Count == 0;
            }

            OnPropertyChanged(nameof(NoSearchResults));
        }

        public void AddSpecialRequest()
        {
            RaiseSpecialRequest window = new()
            {
                Owner = Application.Current.MainWindow
            };

            window.ShowDialog();

            if (window.productAdded)
            {
                TurnedProducts.Add(window.NewProduct);
                SearchTerm = window.NewProduct.ProductName;
            }
        }

        public void SubmitRequest()
        {
            if (SelectedProduct is null)
            {
                MessageBox.Show("Selected product is null", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            newRequest.RaisedBy = App.CurrentUser.GetFullName();
            newRequest.DateRaised = DateTime.Now;
            newRequest.Product = SelectedProduct.ProductName;

            ProductGroup? group = ProductGroups.Find(x => x.Id == SelectedProduct.GroupId);
            Product product = group == null 
                ? null 
                : Products.Find(x => x.Id == group.ProductId);

            if (group is not null)
            {
                if (group.Status == ProductGroup.GroupStatus.Dormant)
                {
                    MessageBox.Show($"This product is a member of a group that is marked as 'Dormant', we are unable to manufacture these.", "Cannot Proceed", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                if (group.Status == ProductGroup.GroupStatus.InDevelopment)
                {
                    MessageBoxResult res = MessageBox.Show($"This product is a member of a group that is marked as 'In Development', it is unlikely a customer requirement can be fulfilled.{Environment.NewLine}Do you want to submit anyway?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }

            string toastImage = product == null 
                ? null 
                : $@"lib\renders\{product.ImageUrl}";

            int newRequestId = DatabaseHelper.Read<sqlite_sequence>().Find(x => x.name == nameof(Request))!.seq + 1;

            // TODO move to notifications manager
            int weeks = (int)Math.Round((NewRequest.DateRequired - DateTime.Now).TotalDays / 7);

            for (int i = 0; i < ToNotify.Count; i++)
            {
                Notification not = new(
                    to: ToNotify[i].UserName,
                    from: App.CurrentUser.UserName,
                    header: "Request Raised",
                    body: $"New request raised for {NewRequest.QuantityRequired:#,##0}pcs of {NewRequest.Product}. Requested in {weeks} weeks.",
                    toastAction: $"viewRequest:{newRequestId:0}",
                    toastImageUrl: toastImage);

                if (not.TargetUser == not.Origin)
                {
                    continue;
                }

                if (!DatabaseHelper.Insert(not))
                {
                    return;
                }
            }

            if (!DatabaseHelper.Insert(NewRequest))
            {
                MessageBox.Show("Failed to add request to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (product is not null)
            {
                string renderPath = product.LocalRenderPath;

                if (renderPath is not null)
                {
                    new ToastContentBuilder()
                       .AddText("New Request Raised")
                       .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                       .AddText("People with approval permissions will get notified.")
                       .AddArgument("action", $"viewRequest:{newRequestId:0}")
                       .AddInlineImage(new Uri(renderPath))
                       .Show();
                }
                else
                {
                    new ToastContentBuilder()
                    .AddText("New Request Raised")
                    .AddArgument("action", $"viewRequest:{newRequestId:0}")
                    .AddText("People with approval permissions will get notified.")
                    .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                    .Show();
                }
            }
            else
            {
                new ToastContentBuilder()
                    .AddText("New Request Raised")
                    .AddArgument("action", $"viewRequest:{newRequestId:0}")
                    .AddText("People with approval permissions will get notified.")
                    .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                    .Show();
            }

        }

        public class LeadTime
        {
            public Lathe Lathe { get; set; }
            public TimeSpan Workload { get; set; }

            public bool IsHigh
            {
                get
                {
                    return Math.Round(Workload.TotalDays / 7 + 1) >= 6; // 6 weeks or more
                }
            }

            public string DisplayLeadTime
            {
                get { return $"{Math.Round(Workload.TotalDays / 7 + 1):0} weeks"; }
            }
        }
    }
}
