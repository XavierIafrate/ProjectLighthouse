using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectLighthouse.Model.Product;

namespace ProjectLighthouse.ViewModel
{
    public class CheckSheetsViewModel : BaseViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<ToolingGroup> ToolingGroups { get; set; } = new();    
        public List<TurnedProduct> TurnedProducts { get; set; } = new();
        public List<CheckSheetField> CheckSheetFields { get; set; } = new();

        private Product selectedProduct = new();
        public Product SelectedProduct
        {
            get { return selectedProduct; }
            set 
            { 
                selectedProduct = value;
                OnPropertyChanged();
            }
        }

        public OpenUrlCommand OpenWebCommand { get; set; }

        public CheckSheetsViewModel()
        {
            InitialiseVariables();
            LoadData();
        }

        public void InitialiseVariables()
        {
            OpenWebCommand = new(this);
        }

        public void LoadData()
        {
            Products = DatabaseHelper.Read<Product>();
            ToolingGroups = DatabaseHelper.Read<ToolingGroup>();
            TurnedProducts = DatabaseHelper.Read<TurnedProduct>()
                .Where(x => !x.isSpecialPart)
                .ToList();
            CheckSheetFields = DatabaseHelper.Read<CheckSheetField>();  

            for (int i = 0; i < ToolingGroups.Count; i++)
            {
                ToolingGroups[i].Products = TurnedProducts
                    .Where(x => x.ProductGroup == ToolingGroups[i].Name)
                    .ToList();

                ToolingGroups[i].CheckSheetFields = CheckSheetFields.Where(x => x.ToolingGroup == ToolingGroups[i].Name).ToList();
            }

            for (int i = 0; i < Products.Count; i++)
            {
                Products[i].ToolingGroups = ToolingGroups
                    .Where(x => x.MemberOf == Products[i].Name)
                    .ToList();
                //Products[i].CheckSheetFields = CheckSheetFields.Where(x => x.Product == Products[i].Name && string.IsNullOrEmpty(x.ToolingGroup)).ToList();
            }

            if (Products.Count > 0)
            {
                SelectedProduct =Products.First();
            }
        }

        public void OpenWebsite(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
