using ProjectLighthouse.Model.Logistics;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class LogisticsKioskWindow : Window, INotifyPropertyChanged
    {

        public string UserName { get; set; }
        public string WorkStation { get; set; }
        public List<PackageRecord> Packages { get; set; }
        public LogisticsKioskWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoginRoutine()
        {
            if (App.CurrentUser == null)
            {
                LoginWindow login = new(logistics: true);

                login.ShowDialog();

                if (login.auth_user == null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    App.CurrentUser = login.auth_user;
                    DataContext = this;
                    UserName = App.CurrentUser.GetFullName();
                    OnPropertyChanged(nameof(UserName));
                    DatabaseHelper.DatabasePath = $"{App.ROOT_PATH}logistics.db3";
                    LoadData();
                    Show();
                }
            }
        }

        private void LoadData()
        {
            Packages = DatabaseHelper.Read<PackageRecord>();
            OnPropertyChanged(nameof(Packages));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RecordPackageButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.Insert(new PackageRecord()
            {
                PackedBy = UserName,
                TimeStamp = DateTime.Now,
                WorkStation = "PACKINGS STATION 00",
                MachineName = Environment.MachineName,
                DomainUser = $"{Environment.UserDomainName}/{Environment.UserName}",
                NumPackages = 1,
                OrderReference = "SOR12345"
                
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void NumbersOnly(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }
    }
}
