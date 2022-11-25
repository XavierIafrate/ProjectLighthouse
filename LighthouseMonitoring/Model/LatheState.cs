using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.CompilerServices;

namespace LighthouseMonitoring.Model
{
    public class LatheState : INotifyPropertyChanged
    {
        public Lathe Lathe { get; set; }
        public MachineData State { get; set; }

        public event EventHandler<List<string>>? OnNewError;
        public event EventHandler<MachineStatus>? OnStatusChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public LatheState(Lathe lathe)
        {
            Lathe = lathe;
            State = new(Lathe);
        }

        public void Update(MachineData newState)
        {
            if (State == null)
            {
                State = newState;
                return;
            }

            MachineData previousState = State;
            State = newState;


            if (newState.Status == MachineStatus.Breakdown && previousState.Status != MachineStatus.Breakdown)
            {
                NotifyError();
            }

            OnPropertyChanged(nameof(State));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyError()
        {
            OnNewError?.Invoke(this, State.Errors);
        }

        private void NotifyStateChanged()
        {
            OnStatusChanged?.Invoke(this, State.Status);
        }

    }
}
