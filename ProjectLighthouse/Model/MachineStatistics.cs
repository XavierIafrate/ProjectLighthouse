using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model
{
    public class MachineStatistics
    {
        public string MachineID { get; set; }
        public string MachineName { get; set; }

        private DateTime dataTime;
        public DateTime DataTime
        {
            get { return dataTime; }
            set
            {
                dataTime = value;
                ExcelDataTime = String.Format("{0:dd/MM/yyyy HH:mm:ss}", value);
            }
        }
        public string ExcelDataTime { get; set; }

        public string SerialNumber { get; set; }

        public string Availability { get; set; }

        public string ControllerMode { get; set; }
        public string Execution { get; set; }
        public string Block { get; set; }


        public string EmergencyStop { get; set; }
        public int PartCountAll { get; set; }
        public int PartCountRemaining { get; set; }
        public int PartCountTarget { get; set; }
        public string Program { get; set; }

        public int CycleTime { get; set; }
        public int CuttingTime { get; set; }

        public string SystemMessages { get; set; }
        private string status;

        public string Status
        {
            get
            {
                if (String.IsNullOrEmpty(status))
                {
                    status = "Unknown";
                }
                return status;
            }

            set { status = value; }
        }


        public bool HasError()
        {
            return Status == "Breakdown";
        }

        public static string GetError()
        {
            return "debug_err";
        }

        private List<string> getErrors()
        {
            List<string> results = new();

            if (SystemMessages != "")
            {
                results = SystemMessages.Split(";").ToList();
            }
            return results;
        }

        public void SetStatus()
        {

            //Add Idle at some point

            if (SerialNumber == "UNAVAILABLE") // Powered down
            {
                Status = "Offline";
                return;
            }

            List<string> errors = getErrors();
            foreach (var error in errors)
            {
                if (error.ToUpper().Contains("SETTING"))
                {
                    Status = "Setting";
                    return;
                }
                if (error.ToUpper().Contains("ALARM"))
                {
                    Status = "Breakdown";
                    return;
                }
            }

            // Running, Setting, Breakdown, Offline
            if (ControllerMode.Contains("MANUAL") || SystemMessages.ToUpper().Contains("SETTING") || SystemMessages.ToUpper().Contains("PAUSE SIGNAL ON"))
            {
                Status = "Setting";
                return;
            }

            if (EmergencyStop == "TRIGGERED")
            {
                Status = "Breakdown";
                return;
            }


            if (Availability == "AVAILABLE" && ControllerMode == "AUTOMATIC" && EmergencyStop == "ARMED")
            {
                Status = "Running";
                return;
            }
            return;

        }



        public bool IsConnected()
        {
            return !String.IsNullOrEmpty(EmergencyStop);
        }

        public string EstimateCompletion()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            if (PartCountRemaining == 0)
            {
                return "-";
            }
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return string.Format("{0}d {1:D2}h {2:D2}m ({3:dddd d}{4} {3:MMMM HH:mm})", t.Days, t.Hours, t.Minutes, finishDate, GetDaySuffix(finishDate.Day));
        }

        public string EstimateCompletionDate()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            if (PartCountRemaining == 0)
            {
                return "-";
            }
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return string.Format("{0:ddd d}{1} {0:MMM HH:mm}", finishDate, GetDaySuffix(finishDate.Day));
        }

        public string EstimateCompletionTimeRemaining()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            if (PartCountRemaining == 0)
            {
                return "-";
            }
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);

            return string.Format("{0:D2}d {1:D2}h {2:D2}m", t.Days, t.Hours, t.Minutes);
        }

        private static string GetDaySuffix(int day)
        {
            return day switch
            {
                1 or 21 or 31 => "st",
                2 or 22 => "nd",
                3 or 23 => "rd",
                _ => "th",
            };
        }

        public List<Variables> GetListOfStatistics()
        {
            List<Variables> variables = new();

            variables.Add(new Variables
            {
                Name = "Data Time",
                TextValue = String.Format("{0:dd/MM/yy HH:mm:ss}", DataTime)
            });
            variables.Add(new Variables
            {
                Name = "Availability",
                TextValue = Availability
            });
            variables.Add(new Variables
            {
                Name = "Controller Mode",
                TextValue = ControllerMode
            });
            variables.Add(new Variables
            {
                Name = "Emergency Stop",
                TextValue = EmergencyStop
            });

            return variables;
        }
    }
    public class Variables
    {
        public string Name { get; set; }
        public string TextValue { get; set; }
    }
}