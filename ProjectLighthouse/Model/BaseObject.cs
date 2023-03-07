using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model
{
    public class BaseObject : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region INotifyDataErrorInfo Members

        [Ignore]
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public bool HasErrors => Errors.Any();


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            // TODO verify this isn't causing problems
            if (propertyName is null) return null;

            if (!Errors.ContainsKey(propertyName))
            {
                return null;
            }

            return Errors[propertyName];
        }
        public bool NoErrors => !HasErrors;

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(NoErrors));
        }

        protected void ClearErrors(string propertyName)
        {
            if (Errors.ContainsKey(propertyName))
            {
                Errors.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }

        protected void AddError(string propertyName, string error)
        {
            if (!Errors.ContainsKey(propertyName))
                Errors[propertyName] = new List<string>();

            if (!Errors[propertyName].Contains(error))
            {
                Errors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        #endregion

        public class UpdateWatch : Attribute
        {

        }

        public class Import : Attribute
        {
            public string Name { get; set; }
            public Import(string name)
            {
                Name = name;
            }
        }
    }
}
