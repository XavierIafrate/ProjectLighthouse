using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LighthouseMonitoring.Model
{
    public class MachineData
    {
        public string MachineId { get; set; }
        public string MachineName { get; set; }

        public DateTime DataTime { get; set; }

        public MachineData(Lathe lathe)
        {
            MachineId = lathe.Id;
            MachineName = lathe.FullName;
        }
    }
}
