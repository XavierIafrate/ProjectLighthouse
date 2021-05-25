using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class BillOfMaterialsViewModel : BaseViewModel
    {
        public List<BillOfMaterials> BOMs { get; set; }
        private BillOfMaterials selectedBOM;
        public BillOfMaterials SelectedBOM
        {
            get { return selectedBOM; }
            set 
            { 
                selectedBOM = value;
                NoneFoundVis = selectedBOM.Items.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("SelectedBOM");
            }
        }

        private Visibility noneFoundVis;

        public Visibility NoneFoundVis
        {
            get { return noneFoundVis; }
            set 
            {
                noneFoundVis = value;
                OnPropertyChanged("NoneFoundVis");
            }
        }


        public BillOfMaterialsViewModel()
        {
            BOMs = new List<BillOfMaterials>();
            LoadData();
            if (BOMs.Count() > 0)
                SelectedBOM = BOMs.First();
        }

        private void LoadData()
        {
            List<BillOfMaterialsItem> materialItems = DatabaseHelper.Read<BillOfMaterialsItem>().ToList();
            List<AssemblyItem> products = DatabaseHelper.Read<AssemblyItem>().ToList();

            foreach(AssemblyItem product in products)
            {
                if (string.IsNullOrEmpty(product.BillOfMaterials))
                    continue;
                BOMs.Add(new BillOfMaterials()
                {
                    ID = product.BillOfMaterials,
                    ToMake = product.ProductNumber, 
                    Items = materialItems.Where(n => n.BOMID == product.BillOfMaterials).ToList()
                });
            }

        }
    }
}
