using System;

namespace ProjectLighthouse.Model
{
    public class MachineOperatingBlock
    {
        public string MachineID { get; set; }
        public string MachineName { get; set; }
        public string State { get; set; }
        public DateTime StateEntered { get; set; }
        public DateTime StateLeft { get; set; }
        public double SecondsElapsed { get; set; }
    }
}
