using ProjectLighthouse.Model.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.ViewModel.Core
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void DeleteNote(Note note)
        {

        }

        public virtual void UpdateNote(Note note)
        {

        }

        public virtual bool CanClose()
        {
            return true;
        }
    }
}
