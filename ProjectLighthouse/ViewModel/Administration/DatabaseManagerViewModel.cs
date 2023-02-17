using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModel.Commands.Administration;

namespace ProjectLighthouse.ViewModel.Administration
{
    public class DatabaseManagerViewModel : BaseViewModel
    {
        public List<LatheManufactureOrder> Orders { get; set; }

        public GetRecordsAsCsvCommand GetRecordsAsCsvCmd { get; set; }

        public DatabaseManagerViewModel()
        {
            GetRecordsAsCsvCmd = new(this);

            Orders = DatabaseHelper.Read<LatheManufactureOrder>();

        }

        public void GetCsv(string type)
        {
            switch (type)
            {
                case "Orders":
                    CSVHelper.WriteListToCSV(Orders, "orders");
                        break;
                case "WorkloadOrders":
                    CSVHelper.WriteListToCSV(Orders
                        .Where(x => 
                            x.State < OrderState.Cancelled 
                            && x.StartDate.Date.AddMonths(18) > DateTime.Now 
                            && !string.IsNullOrEmpty(x.AllocatedMachine)
                            )
                        .OrderBy(x => x.StartDate)
                        .ToList(), "workloadOrders");
                    break;
                default:
                    throw new NotImplementedException();
            };
        }
    }
}
