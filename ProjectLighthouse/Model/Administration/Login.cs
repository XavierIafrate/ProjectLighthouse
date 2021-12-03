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
        public string Role { get; set; }
        public DateTime Time { get; set; }
        public string Version { get; set; }
    }
}
