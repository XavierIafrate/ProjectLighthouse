using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.View;
using ProjectLighthouse.View.AssemblyViews;
using ProjectLighthouse.ViewModel.Commands.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel
{
    public class AssemblyProductsViewModel : BaseViewModel
    {
        #region Variables
        public List<AssemblyItem> items { get; set; }
        public List<Routing> routingItems { get; set; }
        public List<BillOfMaterialsItem> materialItems { get; set; }
        public List<CompleteAssemblyProduct> subAssemblies { get; set; }

        //private List<AssemblyItem> potentialBOMItems;
        //public List<AssemblyItem> PotentialBOMItems
        //{
        //    get { return potentialBOMItems; }
        //    set { potentialBOMItems = value; }
        //}

        //public AssemblyItem selectedPotentialBOM { get; set; }

        public ICommand NewProductCommand { get; set; }
        //public ICommand NewBOMCommand { get; set; }
        public ICommand NewRoutingCommand { get; set; }
        //public ICommand AddProductToBOMCommand { get; set; }

        #region Visibilities
        //private Visibility newBOMMenuVis;
        //public Visibility NewBOMMenuVis
        //{
        //    get { return newBOMMenuVis; }
        //    set
        //    {
        //        newBOMMenuVis = value;
        //        OnPropertyChanged("NewBOMMenuVis");
        //    }
        //}
        private Visibility newBOMVis;
        public Visibility NewBOMVis
        {
            get { return newBOMVis; }
            set
            {
                newBOMVis = value;
                OnPropertyChanged("NewBOMVis");
            }
        }

        private Visibility subAssembliesVis;
        public Visibility SubAssembliesVis
        {
            get { return subAssembliesVis; }
            set
            {
                subAssembliesVis = value;
                OnPropertyChanged("SubAssembliesVis");
            }
        }

        private Visibility newRoutingVis;
        public Visibility NewRoutingVis
        {
            get { return newRoutingVis; }
            set
            {
                newRoutingVis = value;
                OnPropertyChanged("NewRoutingVis");
            }
        }
        #endregion
        private CompleteAssemblyProduct currentProduct;
        public CompleteAssemblyProduct CurrentProduct
        {
            get { return currentProduct; }
            set
            {
                currentProduct = value;
                OnPropertyChanged("CurrentProduct");
            }
        }

        private AssemblyItem selecteditem;
        public AssemblyItem SelectedItem
        {
            get { return selecteditem; }
            set
            {
                selecteditem = value;
                CurrentProduct = GetCompleteProductFromProduct(value);
                subAssemblies = GetSubAssembliesFromCompleteProduct(CurrentProduct, true);
                OnPropertyChanged("subAssemblies");
                if (CurrentProduct.routings.RoutingItems == null)
                {
                    NewRoutingVis = Visibility.Visible;
                }
                else
                {
                    NewRoutingVis = CurrentProduct.routings.RoutingItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                if (CurrentProduct.materials.Items == null)
                {
                    NewBOMVis = Visibility.Visible;
                }
                else
                {
                    NewBOMVis = currentProduct.materials.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }


                SubAssembliesVis = subAssemblies.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
                //NewBOMMenuVis = Visibility.Collapsed;
            }
        }

        #endregion

        public AssemblyProductsViewModel()
        {
            items = new List<AssemblyItem>();
            subAssemblies = new List<CompleteAssemblyProduct>();
            //PotentialBOMItems = new List<AssemblyItem>();
            CurrentProduct = new CompleteAssemblyProduct()
            {
                product = new AssemblyItem(),
                materials = new BillOfMaterials(),
                routings = new Routings()
            };

            NewProductCommand = new NewAssemblyProductCommand(this);
            //NewBOMCommand = new NewBillOfMaterialsCommand(this);
            NewRoutingCommand = new NewRoutingCommand(this);
            //AddProductToBOMCommand = new AddProductToBillOfMaterialsCommand(this);

            NewRoutingVis = new Visibility();
            NewBOMVis = new Visibility();
            SubAssembliesVis = new Visibility();
            //NewBOMMenuVis = new Visibility();


            LoadData();
            if (items.Count > 0)
                SelectedItem = items.First();
        }

        public List<CompleteAssemblyProduct> GetSubAssembliesFromCompleteProduct(CompleteAssemblyProduct product, bool unlimitedDepth) // implement unlimited depth
        {
            List<CompleteAssemblyProduct> assemblies = new List<CompleteAssemblyProduct>();
            if (product.materials.ID == null)
                return assemblies;

            foreach (BillOfMaterialsItem item in product.materials.Items)
            {
                CompleteAssemblyProduct _tmp = GetCompleteProductFromProduct(GetProductFromBOMItem(item));
                if (_tmp.materials.Items.Count > 0)
                    assemblies.Add(_tmp);
            }

            return assemblies;
        }

        #region Helpers
        public AssemblyItem GetProductFromBOMItem(BillOfMaterialsItem BOMitem)
        {
            AssemblyItem assemblyItem = new AssemblyItem();

            foreach (AssemblyItem item in items)
            {
                if (item.ProductNumber == BOMitem.ComponentItem)
                {
                    assemblyItem = item;
                    break;
                }
            }
            return assemblyItem;
        }

        public CompleteAssemblyProduct GetCompleteProductFromProduct(AssemblyItem assemblyItem)
        {
            CompleteAssemblyProduct _product = new CompleteAssemblyProduct()
            {
                product = assemblyItem,
                materials = new BillOfMaterials(),
                routings = new Routings()
            };

            if (assemblyItem == null)
                return _product;

            _product.materials = new BillOfMaterials()
            {
                ID = _product.product.BillOfMaterials,
                Items = materialItems.Where(n => n.BOMID == _product.product.BillOfMaterials).ToList()
            };

            _product.routings = new Routings()
            {
                ID = _product.product.Routing,
                RoutingItems = routingItems.Where(n => n.RoutingID == _product.product.Routing).ToList()
            };

            return _product;
        }
        #endregion

        #region new BOM
        //public void AddProductToBOM()
        //{
        //    MessageBox.Show(string.Format("Adding {0}", selectedPotentialBOM.ProductNumber));
        //    CurrentProduct.materials.Items.Add(new BillOfMaterialsItem()
        //    {
        //        ForProduct = currentProduct.product.ProductNumber,
        //        ComponentItem = selectedPotentialBOM.ProductNumber,
        //        Quantity = 1,
        //        Units = currentProduct.product.Units
        //    });
        //    OnPropertyChanged("CurrentProduct");
        //}

        //public void CreateNewBillOfMaterials()
        //{

        //    //NewBOMVis = Visibility.Collapsed;
        //    NewBOMMenuVis = Visibility.Visible;
        //    CalculatePotentialBOMItems();
        //}

        //public void CalculatePotentialBOMItems()
        //{
        //    List<AssemblyItem> availableItems = new List<AssemblyItem>(items.Where(n => n.ProductNumber != CurrentProduct.product.ProductNumber));

        //    PotentialBOMItems.Clear();
        //    List<BillOfMaterialsItem> quarantineList = 
        //        materialItems.Where(n => n.ComponentItem == CurrentProduct.product.ProductNumber).ToList();

        //    if (quarantineList.Count == 0)
        //        PotentialBOMItems = availableItems;

        //    PotentialBOMItems = GetParentItems(quarantineList);

        //    OnPropertyChanged("PotentialBOMItems");
        //}

        //public List<AssemblyItem> GetParentItems(List<BillOfMaterialsItem> quarantine)
        //{
        //    List<AssemblyItem> badReferences = new List<AssemblyItem>();
        //    while (HasItemsWithParents(quarantine))
        //    {
        //        quarantine.AddRange(GetParents(quarantine));
        //    }

        //    List<string> components = new List<string>();
        //    foreach (BillOfMaterialsItem i in quarantine)
        //    {
        //        badReferences.Add(GetProductFromBOMItem(i));
        //    }

        //    foreach(BillOfMaterialsItem i in quarantine)
        //    {
        //        AssemblyItem _product = GetProductFromProductName(i.ForProduct);
        //        if (!badReferences.Contains(_product))
        //            badReferences.Add(_product);
        //    }

        //    return items.Except(badReferences).ToList();
        //}

        //private AssemblyItem GetProductFromProductName(string productName)
        //{
        //    return items.Where(n => n.ProductNumber == productName).FirstOrDefault();
        //}

        //public bool HasItemsWithParents(List<BillOfMaterialsItem> list)
        //{
        //    int i = 0;
        //    List<BillOfMaterialsItem> parents = new List<BillOfMaterialsItem>();
        //    foreach (BillOfMaterialsItem item in list)
        //    {
        //        parents.Clear();
        //        parents = materialItems.Where(p => p.ComponentItem == item.ForProduct).ToList();
        //        if (parents.Count == 0)
        //            continue;
        //        parents = parents.Except(list).ToList();
        //        i += parents.Count();
        //    }
        //    return i != 0;
        //}

        //public List<BillOfMaterialsItem> GetParents(List<BillOfMaterialsItem> list)
        //{
        //    List<BillOfMaterialsItem> parents = new List<BillOfMaterialsItem>();
        //    List<BillOfMaterialsItem> tmp = new List<BillOfMaterialsItem>();

        //    foreach (BillOfMaterialsItem item in list)
        //    {
        //        tmp.Clear();
        //        tmp = materialItems.Where(p => p.ComponentItem == item.ForProduct).ToList();
        //        if (tmp.Count == 0)
        //            continue;
        //        tmp = tmp.Where(n => list.Any(n1 => n1 != n)).ToList();
        //        parents.AddRange(tmp);
        //    }
        //    return parents;
        //}
        #endregion

        #region New Routing

        public void CreateNewRouting()
        {
            AddNewRoutingWindow window = new AddNewRoutingWindow();
            window.Product = CurrentProduct.product.Clone();
            window.ShowDialog();
            if (window.wasSaved)
            {
                routingItems = DatabaseHelper.Read<Routing>().ToList();
                SelectedItem = SelectedItem; //refresh
            }

        }

        #endregion

        public void AddNewProduct()
        {
            NewAssemblyProductWindow window = new NewAssemblyProductWindow();
            window.ShowDialog();
            //MessageBox.Show(string.Format("ProductAdded: {0}", window.addedNew));
            if (!window.addedNew)
                return;

            LoadData();
            OnPropertyChanged("items");
            if (items.Count > 0)
                SelectedItem = items.First();

        }

        public void LoadData()
        {
            items = DatabaseHelper.Read<AssemblyItem>().ToList();
            routingItems = DatabaseHelper.Read<Routing>().ToList();
            materialItems = DatabaseHelper.Read<BillOfMaterialsItem>().ToList();
        }
    }
}
