using ProjectLighthouse.Model.Logistics;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace ProjectLighthouse.View
{
    public partial class LogisticsKioskWindow : Window, INotifyPropertyChanged
    {

        private Brush orderRefValidBrush;
        public Brush OrderRefValidBrush
    {
            get { return orderRefValidBrush; }
            set 
            { 
                orderRefValidBrush = value;
                OnPropertyChanged();
            }
        }

        private string orderRef = "";
        public string OrderRef
        {
            get { return orderRef; }
            set 
            { 
                orderRef = value;
                OnPropertyChanged();
            }
        }

        private Brush qtyValidBrush;

        public Brush QtyValidBrush
    {
            get { return qtyValidBrush; }
            set 
            { 
                qtyValidBrush = value;
                OnPropertyChanged();
            }
        }


        private string qtyText = "";
        public string QtyText
        {
            get { return qtyText; }
            set 
            { 
                qtyText = value;
                OnPropertyChanged();
            }
        }


        public string UserName { get; set; }
        public string WorkStation { get; set; }
        private List<PackageRecord> packages;

        public List<PackageRecord> Packages
    {
            get { return packages; }
            set 
            { 
                packages = value;
                OnPropertyChanged();
            }
        }

        public LogisticsKioskWindow()
        {
            InitializeComponent();
            OrderRefValidBrush = (Brush)Application.Current.Resources["Surface"];
            QtyValidBrush = (Brush)Application.Current.Resources["Surface"];
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
            Packages = DatabaseHelper.Read<PackageRecord>().Where(x => x.TimeStamp.Date == DateTime.Today).ToList();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool InputsAreValid()
        {
            Brush errorBrush = (Brush)Application.Current.Resources["Red"];
            Brush okBrush = (Brush)Application.Current.Resources["Surface"];
            bool result = true;

            if(string.IsNullOrEmpty(OrderRef.Trim()))
            {
                OrderRefValidBrush = errorBrush;
                result = false;
            }
            else
            {
                OrderRefValidBrush = okBrush;
            }

            if (!int.TryParse(QtyText.Trim(), out int n))
            {
                QtyValidBrush = errorBrush;
                result = false;
            }
            else if (n == 0)
            {
                QtyValidBrush = errorBrush;
                result = false;
            }
            else
            {
                QtyValidBrush = okBrush;
            }

            return result;
        }

        private void RecordPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputsAreValid())
            {
                return;
            }
            PackageRecord newRecord = new()
            {
                PackedBy = UserName,
                TimeStamp = DateTime.Now,
                WorkStation = WorkStation,
                MachineName = Environment.MachineName,
                DomainUser = $"{Environment.UserDomainName}/{Environment.UserName}",
                NumPackages = 1,
                OrderReference = OrderRef
            };
            DatabaseHelper.Insert(newRecord);
            Packages.Add(newRecord);
            Packages = new(Packages.Where(x => x.TimeStamp.Date == DateTime.Today));
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
