using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLighthouse.Model
{
    public class MachineStatistics
    {
        public string MachineID { get; set; }
        public DateTime DataTime { get; set; }
        public string SerialNumber { get; set; }
        public string Availability { get; set; }
        public string Program { get; set; }
        public string ControllerMode { get; set; }
        public string ControllerModeOverride { get; set; }
        public string EmergencyStop { get; set; }
        public int PartCountAll { get; set; }
        public int PartCountRemaining { get; set; }
        public int PartCountTarget { get; set; }
        public int CycleTime { get; set; }

        public string EstimateCompletion()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return string.Format("{0}d {1:D2}h {2:D2}m ({3:dddd d}{4} {3:MMMM HH:mm})", t.Days, t.Hours, t.Minutes, finishDate, GetDaySuffix(finishDate.Day));
        }

        public string EstimateCompletionDate()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return string.Format("{0:dddd d}{1} {0:MMMM HH:mm}",finishDate, GetDaySuffix(finishDate.Day));
        }

        public string EstimateCompletionTimeRemaining()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return string.Format("{0:D2}d {1:D2}h {2:D2}m", t.Days, t.Hours, t.Minutes);
        }

        private string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}