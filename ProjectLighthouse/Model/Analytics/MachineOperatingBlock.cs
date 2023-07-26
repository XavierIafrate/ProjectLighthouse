using ProjectLighthouse.Model.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Analytics
{
    public class MachineOperatingBlock : ICloneable
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
        public enum States { Running, Setting, Breakdown, Idle, Offline, Unknown }

        public int GetCalculatedPartsProduced()
        {
            return State != "Running" ? 0 : (int)Math.Floor(SecondsElapsed / (CycleTime < 10 ? 120 : CycleTime));
        }

        public List<string> GetListOfErrors()
        {
            if (ErrorMessages == null)
            {
                return new();
            }

            List<string> results = ErrorMessages.Split(";").ToList();
            List<string> cleanedResults = new();
            foreach (string result in results)
            {
                cleanedResults.Add(result.Trim().Replace("\t", " "));
            }

            return cleanedResults
                .Where(
                    x => !x.Contains("T02 Auto operation pause signal ON")
                            && !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MachineOperatingBlock>(serialised);
        }
    }
}
