using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProjectLighthouse.ViewModel.Commands.Requests
{
    public class PricingEstimateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RequestViewModel viewModel;

        public PricingEstimateCommand(RequestViewModel vm)
        {
            this.viewModel = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.OpenPriceEstimator();
        }
    }
}
