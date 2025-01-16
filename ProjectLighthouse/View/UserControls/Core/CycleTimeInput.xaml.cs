using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.UserControls.Core
{
    public partial class CycleTimeInput : UserControl, INotifyPropertyChanged
    {
        public int CycleTime
        {
            get { return (int)GetValue(CycleTimeProperty); }
            set { SetValue(CycleTimeProperty, value); }
        }

        public static readonly DependencyProperty CycleTimeProperty =
            DependencyProperty.Register("CycleTime", typeof(int), typeof(CycleTimeInput), new PropertyMetadata(-1, SetValues));

        private bool loaded;

        private int secondsPart;

        public int SecondsPart
        {
            get { return secondsPart; }
            set
            {
                if (value > 59) return;
                secondsPart = value;
                UpdateCycleTime();
                OnPropertyChanged();
            }
        }

        private void UpdateCycleTime()
        {
            if (!loaded) return;
            CycleTime = SecondsPart + MinutesPart * 60;
            OnPropertyChanged(nameof(CycleTime));
        }

        private int minutesPart;
        public int MinutesPart
        {
            get { return minutesPart; }
            set { minutesPart = value; UpdateCycleTime(); OnPropertyChanged(); }
        }

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not CycleTimeInput control) return;
            control.loaded = false;
            control.InitialiseCycleTime();
            control.loaded = true;
        }

        private void InitialiseCycleTime()
        {
            SecondsPart = CycleTime % 60;
            MinutesPart = (int)Math.Floor((double)CycleTime / 60);
        }

        public CycleTimeInput()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
