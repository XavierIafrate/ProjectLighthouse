﻿using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class RequestsToCSVCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private RequestViewModel viewModel;

        public RequestsToCSVCommand(RequestViewModel vm)
        {
            viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return App.currentUser.UserRole == "Purchasing" || App.currentUser.UserRole == "Scheduling" || App.currentUser.UserRole == "admin";
        }

        public void Execute(object parameter)
        {
            viewModel.ExportRequestsToCSV();
        }
    }
}