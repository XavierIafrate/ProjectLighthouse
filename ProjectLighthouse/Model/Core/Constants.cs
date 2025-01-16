using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Core
{
    public class Constants
    {
        // Cost of machine time per second
        public double AbsorptionRate { get; set; }

        public string BaseProgramPath { get; set; } = @"\\groupfile01\Sales\Production\Programs\Citizen\Part Programs\";

        public int BarRemainder { get; set; } = 300;
        public int BarRequisitionDays { get; set; } = 14;
        public int DefaultSettingTime { get; set; } = 6;

        public double MaxPartLength { get; set; }
        public double MaxPartDiameter { get; set; }


        public OpeningHours OpeningHours { get; set; }


        public Constants()
        {
            
        }

        internal void SetLatheValues()
        {
            // Automated inputs
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>();
            MaxPartLength = lathes.Max(x => x.MaxLength);
            MaxPartDiameter = lathes.Max(x => x.MaxDiameter);
        }
    }
}
