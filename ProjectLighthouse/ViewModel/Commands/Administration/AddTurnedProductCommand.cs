﻿using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class AddTurnedProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private ProductManagerViewModel viewModel;
        public AddTurnedProductCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }


        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(Model.Core.PermissionType.CreateProducts);

        }

        public void Execute(object parameter)
        {
            viewModel.AddTurnedProduct();
        }
    }
}
