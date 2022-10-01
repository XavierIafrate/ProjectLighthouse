using SQLite;
using System;

namespace ProjectLighthouse.Model.Core
{
    public class Login
    {
        [AutoIncrement, PrimaryKey]
        public int ID { get; set; }
        public string User { get; set; }
        public string Host { get; set; }
        public string ADUser { get; set; }
        public string Role { get; set; }
        public DateTime Time { get; set; }
        public string Version { get; set; }
        public DateTime Logout { get; set; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public string GetSessionDuration()
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if (Logout > Time)
            {
                TimeSpan time = Logout - Time;
                return $"{time.Hours}h {time.Minutes}m {time.Seconds}s";
            }
            else
            {
                return null;
            }
        }
    }
}
