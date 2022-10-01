using SQLite;
using System;

namespace ProjectLighthouse.Model.Core
{
    public class Notification
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string TargetUser { get; set; }
        public string Origin { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public bool Seen { get; set; }
        public DateTime SeenTimeStamp { get; set; }
        public string ToastAction { get; set; }
        public string ToastInlineImageUrl { get; set; }

        public Notification(string to, string from, string header, string body, string toastAction = null, string toastImageUrl = null)
        {
            TimeStamp = DateTime.Now;
            Seen = false;
            TargetUser = to;
            Origin = from;
            Header = header;
            Body = body;
            ToastAction = toastAction;
            ToastInlineImageUrl = toastImageUrl;
        }

        public Notification()
        {

        }
    }
}
