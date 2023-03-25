using ProjectLighthouse.Model.Core;
using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Programs
{
    public class NcProgramCommit : BaseObject, IObjectWithValidation, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public int ProgramId { get; set; }
        [Unique]
        public string FileName { get; set; }
        public string CommittedBy { get; set; }
        public DateTime CommittedAt { get; set; }

        private string commitMessage;
        public string CommitMessage
        {
            get { return commitMessage; }
            set 
            { 
                commitMessage = value; 
                ValidateProperty(); 
                OnPropertyChanged(); 
            }
        }

        public string Url
        {
            get
            {
                return $@"{App.ROOT_PATH}lib\pcom\{FileName}.txt";
            }
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(CommitMessage));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(CommitMessage))
            {
                ClearErrors(propertyName);

                if(string.IsNullOrWhiteSpace(CommitMessage))
                {
                    AddError(propertyName, "Commit Message is required");
                }

                return;
            }

            throw new NotImplementedException();
        }
    }
}
