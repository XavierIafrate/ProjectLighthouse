using SQLite;
using System;

namespace ProjectLighthouse.Model.Analytics
{
    public class MachineEvent
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string MachineID { get; set; }
        public string MachineName { get; set; }
        public int EventDuration { get; set; }
        public int CycleTime { get; set; }

        public DateTime EventStarted { get; set; }
        public DateTime EventStopped { get; set; }
        public string MachineState { get; set; }

        public string ErrorMessage { get; set; }

    }
}
