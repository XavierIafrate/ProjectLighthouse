﻿using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Research;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Research
{
    public class OpenRootDirectoryCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private ResearchViewModel viewModel;

        public OpenRootDirectoryCommand(ResearchViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return App.CurrentUser.HasPermission(PermissionType.ModifyProjects);
        }

        public void Execute(object parameter)
        {
            viewModel.OpenRoot();
        }
    }
}