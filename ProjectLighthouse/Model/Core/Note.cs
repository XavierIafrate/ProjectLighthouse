using ProjectLighthouse.Model.Administration;
using SQLite;
using System;

namespace ProjectLighthouse.Model.Core
{
    public class Note : BaseObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged(); }
        }

        public string OriginalMessage { get; set; }
        private bool isEdited;

        public bool IsEdited
        {
            get { return isEdited; }
            set { isEdited = value; OnPropertyChanged(); }
        }


        public string SentBy { get; set; }
        public string DateSent { get; set; }
        public string DateEdited { get; set; }
        public string DocumentReference { get; set; }

        private bool isDeleted;
        public bool IsDeleted
        {
            get { return isDeleted; }
            set { isDeleted = value; OnPropertyChanged();  }
        }


        [Ignore]
        public bool ShowHeader { get; set; }
        [Ignore]
        public bool ShowDateHeader { get; set; }
        [Ignore]
        public bool SpaceUnder { get; set; }
        [Ignore]
        public bool ShowEdit { get; set; }
        [Ignore]
        public User UserDetails { get; set; }

        public event Action Edited;


        public void NotifyEdited()
        {
            Edited?.Invoke();
        }
    }
}
