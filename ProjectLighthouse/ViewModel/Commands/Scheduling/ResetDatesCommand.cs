﻿using ProjectLighthouse.ViewModel.Orders;
using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Scheduling
{
    public class ResetDatesCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private NewScheduleViewModel viewModel;

        public ResetDatesCommand(NewScheduleViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ResetDates();
        }
    }
}