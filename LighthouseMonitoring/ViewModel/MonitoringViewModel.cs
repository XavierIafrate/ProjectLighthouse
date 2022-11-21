using LighthouseMonitoring.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace LighthouseMonitoring.ViewModel
{
    public class MonitoringViewModel : BaseViewModel
    {
        public MonitoringSystem MonitoringSystem { get { return App.MonitoringSystem; } }
        public MonitoringViewModel()
        {
            DisplayText = "Monitoring";
        }
    }
}
