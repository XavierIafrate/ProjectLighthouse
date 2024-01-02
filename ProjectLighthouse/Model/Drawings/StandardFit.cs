using System.Collections.Generic;

namespace ProjectLighthouse.Model.Drawings
{
    public class StandardFit
    {
        public string Symbol { get; set; }
        public List<ToleranceZone> ToleranceZones { get; set; }
        
        public ToleranceZone At(double nominal)
        {
            return ToleranceZones.Find(x => nominal > x.Over && nominal <= x.To);
        }
    }

    public class ToleranceZone
    {
        public int Over { get; set; }
        public int To { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
    }
}
