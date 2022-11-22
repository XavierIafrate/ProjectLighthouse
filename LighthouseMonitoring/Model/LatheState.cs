using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LighthouseMonitoring.Model
{
    public class LatheState
    {
        public Lathe Lathe { get; set; }


        public LatheState(Lathe lathe)
        {
            Lathe = lathe;
        }
    }
}
