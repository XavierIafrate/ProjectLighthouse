﻿using ProjectLighthouse.ViewModel.Administration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Administration
{
    public class EditTurnedProductCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ProductManagerViewModel viewModel;

        public EditTurnedProductCommand(ProductManagerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is not int id)
            {
                return;
            }

            viewModel.EditTurnedProduct(id);
        }
    }
}