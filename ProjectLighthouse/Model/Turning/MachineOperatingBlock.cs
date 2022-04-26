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
        public int CycleTime { get; set; }
        public string Messages { get; set; }
        public string ErrorMessages { get; set; }
        public enum States { Running, Setting, Breakdown, Idle, Offline, Unknown}

        public int GetCalculatedPartsProduced()
        {
            return State != "Running" ? 0 : (int)Math.Floor(SecondsElapsed / CycleTime);
        }
    }
}
