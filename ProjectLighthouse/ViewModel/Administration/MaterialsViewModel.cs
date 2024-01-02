using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System.Collections.Generic;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class MaterialsViewModel : BaseViewModel
    {
        private List<MaterialInfo> materials;

        public List<MaterialInfo> Materials
        {
            get { return materials; }
            set { materials = value; OnPropertyChanged(); }
        }


        public MaterialsViewModel()
        {
            GetData();
        }

        private void GetData()
        {
            Materials = DatabaseHelper.Read<MaterialInfo>();
        }

    }
}
