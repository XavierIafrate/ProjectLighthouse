﻿using System;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands
{
    public class SendMessageCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private BaseViewModel viewModel;

        public SendMessageCommand(BaseViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (viewModel is RequestViewModel rvm)
            {
                rvm.SendMessage();
            }
            else if (viewModel is QualityCheckViewModel qcvm)
            {
                qcvm.SendMessage();
            }
        }
    }
}