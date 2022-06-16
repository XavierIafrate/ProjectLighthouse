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
                ExcelDataTime = $"{value:dd/MM/yyyy HH:mm:ss}";
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

        public string SystemMessages { get; set; } = "";
        private string status;

        public string Status
        {
            get
            {
                if (string.IsNullOrEmpty(status))
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

        public string GetError()
        {
            List<string> errors = GetErrors();
            errors = new List<string>(errors.Where(n => n.Contains("EX")));
            string result = errors.Count > 0 ? errors.First().ToString() : "Unknown";
            return result;
        }

        public List<string> GetErrors()
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
            if (SerialNumber == "UNAVAILABLE") // Powered down
            {
                Status = "Offline";
                return;
            }

            if (SystemMessages == null)
            {
                SystemMessages = "";
            }

            if (!string.IsNullOrEmpty(SystemMessages))
            {
                if (SystemMessages.ToUpper().Contains("WORK COUNTER FULL"))
                {
                    Status = "Idle";
                    return;
                }

                if (SystemMessages.ToUpper().Contains("SETTING"))
                {
                    Status = "Setting";
                    return;
                }

                if (SystemMessages.ToUpper().Contains("ALARM"))
                {
                    Status = "Breakdown";
                    return;
                }
            }

            // Running, Setting, Breakdown, Offline
            if (ControllerMode.Contains("MANUAL") || SystemMessages.ToUpper().Contains("SETTING")) // || SystemMessages.ToUpper().Contains("PAUSE SIGNAL ON")
            {
                Status = "Setting";
                return;
            }

            if (EmergencyStop == "TRIGGERED")
            {
                Status = "Breakdown";
                return;
            }

            if (Availability == "AVAILABLE" && ControllerMode == "AUTOMATIC" && EmergencyStop == "ARMED" && Execution == "ACTIVE")
            {
                Status = "Running";
                return;
            }

            if(Block.Contains("MACHINEID"))
            {
                Status = "Idle";
                return;
            }

            return;
        }



        public bool IsConnected()
        {
            return !string.IsNullOrEmpty(EmergencyStop);
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

            return $"{t.Days}d {t.Hours:D2}h {t.Minutes:D2}m ({finishDate:dddd d}{GetDaySuffix(finishDate.Day)} {finishDate:MMMM HH:mm})";
        }

        public DateTime GetCompletionDateTime()
        {
            return DateTime.Now.AddSeconds(CycleTime * PartCountRemaining);
        }

        public string EstimateCompletionDate()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            if (PartCountRemaining == 0)
            {
                return "-";
            }
            DateTime finishDate = DateTime.Now.AddSeconds(secondsLeft);

            return $"{finishDate:ddd d}{GetDaySuffix(finishDate.Day)} {finishDate:MMM HH:mm}";
        }

        public string EstimateCompletionTimeRemaining()
        {
            int secondsLeft = CycleTime * PartCountRemaining;
            if (PartCountRemaining == 0)
            {
                return "-";
            }
            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);

            return $"{t.Days:D2}d {t.Hours:D2}h {t.Minutes:D2}m";
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
                TextValue = $"{DataTime:dd/MM/yy HH:mm:ss}"
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