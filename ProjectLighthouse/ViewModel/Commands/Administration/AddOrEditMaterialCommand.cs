using ProjectLighthouse.Model.Material;
using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddOrEditMaterialCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MaterialsViewModel viewModel;

        public AddOrEditMaterialCommand(MaterialsViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.EditMaterials);
        }

        public void Execute(object parameter)
        {
            MaterialInfo? material = null;

            if (parameter is MaterialInfo m)
            {
                material = m;
            }

            viewModel.AddOrEditMaterial(material);
        }
    }
}
