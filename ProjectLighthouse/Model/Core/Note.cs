using ProjectLighthouse.Model.Administration;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Core
{
    public class Note : BaseObject, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string message = string.Empty;
        public string Message
        {
            get { return message; }
            set 
            { 
                message = value;
                OnPropertyChanged(); 
            }
        }

        public string OriginalMessage { get; set; }
        private bool isEdited;

        public bool IsEdited
        {
            get { return isEdited; }
            set { isEdited = value; OnPropertyChanged(); }
        }


        public string SentBy { get; set; } = string.Empty;
        public string DateSent { get; set; } = string.Empty;
        public string DateEdited { get; set; } = string.Empty;
        public string DocumentReference { get; set; } = string.Empty;

        private bool isDeleted;
        public bool IsDeleted
        {
            get { return isDeleted; }
            set { isDeleted = value; OnPropertyChanged(); }
        }


        [Ignore]
        public bool ShowHeader { get; set; }
        [Ignore]
        public bool SpaceUnder { get; set; }
        [Ignore]
        public User UserDetails { get; set; }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Message));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Message))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Message))
                {
                    AddError(propertyName, "Message must not be empty");
                }

                return;
            }
            throw new NotImplementedException();
        }
    }
}
