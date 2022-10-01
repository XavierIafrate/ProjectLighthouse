using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace ProjectLighthouse.View
{
    public partial class ShowError : Window, INotifyPropertyChanged
    {
        public DispatcherUnhandledExceptionEventArgs Error { get; set; }
        public ShowError()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
