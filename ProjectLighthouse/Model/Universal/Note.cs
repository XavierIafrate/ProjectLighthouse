using SQLite;
using System;

namespace ProjectLighthouse.Model
{
    public class Note
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Message { get; set; }
        public string OriginalMessage { get; set; }
        public bool IsEdited { get; set; }

        public string SentBy { get; set; }
        public string DateSent { get; set; }
        public string DateEdited { get; set; }
        public string DocumentReference { get; set; }
        public bool IsDeleted { get; set; }

        [Ignore]
        public bool ShowHeader { get; set; }
        [Ignore]
        public bool ShowEdit { get; set; }

        public event Action PropertyChanged;
    }
}
