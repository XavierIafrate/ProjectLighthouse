using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Drawings
{
    public partial class AddNewDrawingWindow : Window, INotifyPropertyChanged
    {
        private TechnicalDrawing newDrawing;

        public TechnicalDrawing NewDrawing
        {
            get { return newDrawing; }
            set
            {
                newDrawing = value;
                OnPropertyChanged();
            }
        }

        public List<Product> Products { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<TurnedProduct> TurnedProducts { get; set; }
        public List<TechnicalDrawing> Drawings { get; set; }
        public List<MaterialInfo> Materials { get; set; }


        private bool archetypeMode;
        public bool ArchetypeMode
        {
            get { return archetypeMode; }
            set
            {
                archetypeMode = value;

                if (archetypeMode)
                {
                    CustomerIssueMode = false;
                }

                SetArchetypeMode();
                NewDrawing.IsArchetype = value;
                OnPropertyChanged();
            }
        }

        private bool customerIssueMode;

        public bool CustomerIssueMode
        {
            get { return customerIssueMode; }
            set
            {
                customerIssueMode = value;
                if (customerIssueMode)
                {
                    ArchetypeMode = false;
                }
                OnPropertyChanged();

            }
        }




        private List<ProductGroup> filteredGroups;
        public List<ProductGroup> FilteredGroups
        {
            get { return filteredGroups; }
            set
            {
                filteredGroups = value;
                OnPropertyChanged();
            }
        }

        private Product? selectedProduct;
        public Product? SelectedProduct
        {
            get { return selectedProduct; }
            set
            {
                selectedProduct = value;
                FilterGroups();
                OnPropertyChanged();
            }
        }

        private ProductGroup? selectedGroup;
        public ProductGroup? SelectedGroup
        {
            get { return selectedGroup; }
            set
            {
                selectedGroup = value;
                NewDrawing.GroupId = SelectedGroup?.Id;

                FilterProducts();
                FilterMaterials();
                OnPropertyChanged();
            }
        }

        private MaterialInfo? selectedMaterial;
        public MaterialInfo? SelectedMaterial
        {
            get { return selectedMaterial; }
            set
            {
                selectedMaterial = value;
                NewDrawing.MaterialId = SelectedMaterial?.Id;

                FilterProducts();
                OnPropertyChanged();
            }
        }


        private TurnedProduct? selectedTurnedProduct;
        public TurnedProduct? SelectedTurnedProduct
        {
            get { return selectedTurnedProduct; }
            set
            {
                selectedTurnedProduct = value;
                NewDrawing.TurnedProductId = SelectedTurnedProduct?.Id;

                OnPropertyChanged();
            }
        }



        private List<MaterialInfo> filteredMaterials;
        public List<MaterialInfo> FilteredMaterials
        {
            get { return filteredMaterials; }
            set
            {
                filteredMaterials = value;
                OnPropertyChanged();
            }
        }

        private List<TurnedProduct> filteredTurnedProducts = new();

        public List<TurnedProduct> FilteredTurnedProducts
        {
            get { return filteredTurnedProducts; }
            set
            {
                filteredTurnedProducts = value;
                OnPropertyChanged();
            }
        }


        private string targetFilePath;
        public string TargetFilePath
        {
            get { return targetFilePath; }
            set
            {
                targetFilePath = value;
                OnPropertyChanged();
            }
        }

        private string searchText = "";
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                FilterProducts();
                OnPropertyChanged();
            }
        }

        public bool SaveExit;

        public AddNewDrawingWindow(List<TechnicalDrawing> drawings)
        {
            InitializeComponent();
            Drawings = drawings;
            LoadData();
            CustomerIssueCheckBox.Visibility = App.CurrentUser.Role == UserRole.Administrator
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void LoadData()
        {
            NewDrawing = new();
            ArchetypeMode = false;

            Products = DatabaseHelper.Read<Product>()
                .OrderBy(x => x.Name)
                .ToList();
            ProductGroups = DatabaseHelper.Read<ProductGroup>()
                .OrderBy(x => x.Name)
                .ToList();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .Where(x => !x.Retired)
                .OrderBy(x => x.ProductName)
                .ToList();
            Materials = DatabaseHelper.Read<MaterialInfo>();
        }

        private void SetArchetypeMode()
        {
            if (ArchetypeMode)
            {
                NewDrawing.IsArchetype = true;
                SearchText = "";
                FilteredTurnedProducts = new();
            }
            else
            {
                NewDrawing.IsArchetype = false;
                SelectedProduct = null;
                FilteredTurnedProducts = new();
            }
        }

        private void FilterGroups()
        {
            if (SelectedProduct is null)
            {
                FilteredGroups = new();
                return;
            }

            FilteredGroups = ProductGroups
                .Where(x => x.ProductId == SelectedProduct.Id)
                .OrderBy(x => x.Name)
                .ToList();
        }

        private void FilterMaterials()
        {
            if (SelectedGroup is null)
            {
                FilteredMaterials = new();
                return;
            }

            FilteredMaterials = Materials
                .Where(x => FilteredTurnedProducts.Any(y => y.MaterialId == x.Id))
                .OrderBy(x => x.Id)
                .ToList();
        }

        private void FilterProducts()
        {
            if (ArchetypeMode)
            {
                if (SelectedGroup is null)
                {
                    FilteredTurnedProducts = new();
                    return;
                }

                FilteredTurnedProducts = TurnedProducts
                    .Where(x => x.GroupId == SelectedGroup.Id && !x.IsSpecialPart)
                    .ToList();

                if (SelectedMaterial is not null)
                {
                    FilteredTurnedProducts = FilteredTurnedProducts
                        .Where(x => x.MaterialId == SelectedMaterial.Id)
                        .ToList();
                }

                return;
            }

            // User search
            string userRequest = (SearchText ?? "").ToUpperInvariant().Replace(" ", "");
            List<TurnedProduct> foundProducts = TurnedProducts
                .Where(x => x.ProductName.Contains(userRequest))
                .ToList();

            if (userRequest.Length < 3)
            {
                foundProducts = foundProducts
                    .Where(x => x.ProductName == userRequest)
                    .ToList();
            }

            FilteredTurnedProducts = foundProducts;
        }

        private bool DataOk()
        {
            if (string.IsNullOrWhiteSpace(TargetFilePath))
            {
                MessageBox.Show("Choose a file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (ArchetypeMode)
            {
                if (SelectedGroup is null)
                {
                    MessageBox.Show("You need to select a product group for the archetype", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else
            {
                if (SelectedTurnedProduct is null)
                {
                    MessageBox.Show("You need to select a product", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(NewDrawing.IssueDetails))
            {
                MessageBox.Show("Issue details are required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool CheckNoOpenDrawings()
        {

            List<TechnicalDrawing> pendingDrawings = DatabaseHelper.Read<TechnicalDrawing>().Where(x => x.PendingApproval()).ToList();
            if (ArchetypeMode)
            {
                pendingDrawings = pendingDrawings.Where(x => x.GroupId == SelectedGroup!.Id).ToList();
            }
            else
            {
                pendingDrawings = pendingDrawings.Where(x => x.TurnedProductId == SelectedTurnedProduct!.Id).ToList();
            }

            return pendingDrawings.Count == 0;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NewDrawing.DrawingType = (ResearchCheckbox.IsChecked ?? false) ? TechnicalDrawing.Type.Research : TechnicalDrawing.Type.Production;
            if (!DataOk())
            {
                return;
            }

            bool nothingPending = CheckNoOpenDrawings();
            if (!nothingPending)
            {
                MessageBox.Show("The target drawing has a pending candidate - the candidate must be rejected or withdrawn if you wish to add a new candidate.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            GetDrawingName();

            if (!NewDrawing.MoveToDrawingStore(TargetFilePath))
            {
                MessageBox.Show("An error occurred while moving the file into Lighthouse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewDrawing.IssueDetails = NewDrawing.IssueDetails.Trim();
            NewDrawing.Created = DateTime.Now;
            NewDrawing.CreatedBy = App.CurrentUser.GetFullName();

            if (CustomerIssueMode)
            {
                NewDrawing.WatermarkOnly = true;
            }

            NewDrawing.PrepareMarkedPdf();
            if (!DatabaseHelper.Insert(NewDrawing))
            {
                MessageBox.Show("An error occurred when adding to the database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (!CustomerIssueMode)
            {
                // TODO move to notifications manager
                List<User> ToNotify = App.NotificationsManager.Users.Where(x => x.HasPermission(PermissionType.ApproveDrawings) && x.UserName != App.CurrentUser.UserName).ToList();

                for (int i = 0; i < ToNotify.Count; i++)
                {
                    DatabaseHelper.Insert<Notification>(new(to: ToNotify[i].UserName, from: App.CurrentUser.UserName, header: $"Proposal: {NewDrawing.DrawingName}", body: $"{App.CurrentUser.FirstName} has submitted a proposal for {NewDrawing.DrawingName}, please approve or reject.", toastAction: $"viewDrawing:{NewDrawing.Id}"));
                }
            }

            SaveExit = true;
            Close();
        }

        private void GetDrawingName()
        {
            if (NewDrawing.IsArchetype)
            {
                if (SelectedMaterial is null)
                {
                    NewDrawing.DrawingName = SelectedGroup!.Name;
                }
                else
                {
                    NewDrawing.DrawingName = $"{SelectedGroup!.Name}-{SelectedMaterial.MaterialCode}";
                }
                return;
            }

            NewDrawing.DrawingName = SelectedTurnedProduct!.ProductName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ClearMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMaterial = null;
        }
    }
}
