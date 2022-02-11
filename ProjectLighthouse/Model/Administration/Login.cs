using SQLite;
using System;

namespace ProjectLighthouse.Model
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

        public string? GetSessionDuration()
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
