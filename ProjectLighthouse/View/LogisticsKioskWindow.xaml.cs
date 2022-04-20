using ProjectLighthouse.Model.Logistics;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectLighthouse.View
{
    public partial class LogisticsKioskWindow : Window, INotifyPropertyChanged
    {
        #region Inputs
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
        #endregion

        #region Data
        public string WorkStation { get; set; }

        private List<PackageRecord> allPackages;
        private List<PackageRecord> packages;

        public List<PackageRecord> Packages
        {
            get { return packages; }
            set
            {
                packages = value;
                OnPropertyChanged();
                CalculateAnalytics();
            }
        }

        public string UserFullName { get; set; }
        #endregion

        #region Analytics
        public int PackagesToday { get; set; }
        public int PackagesThisWeek { get; set; }
        public double HourlyRate { get; set; }
        #endregion

        #region Timer
        private Timer insightsRefresher;
        private const int intervalMinutes = 1;
        #endregion

        public LogisticsKioskWindow()
        {
            InitializeComponent();
            OrderRefValidBrush = (Brush)Application.Current.Resources["Surface"];
            QtyValidBrush = (Brush)Application.Current.Resources["Surface"];
            DeleteButton.IsEnabled = false;

            insightsRefresher = new Timer(intervalMinutes * 60000);
            insightsRefresher.Elapsed += OnTimerTick;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            CalculateAnalytics();
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
                    UserFullName = App.CurrentUser.GetFullName();
                    OnPropertyChanged(nameof(UserFullName));
                    DataContext = this;
                    LoadData();
                    Show();
                    insightsRefresher.Start();
                }
            }
        }

        private void CalculateAnalytics()
        {
            //Today
            PackagesToday = Packages
                .Where(x => x.TimeStamp.Date == DateTime.Today)
                .Sum(x => x.NumPackages);
            OnPropertyChanged(nameof(PackagesToday));

            // This Week
            int dayOfWeek = (int)DateTime.Now.DayOfWeek;  // Monday == 1
            DateTime monday = DateTime.Now.Date.AddDays(-1 * (dayOfWeek - 1));
            PackagesThisWeek = Packages
                .Where(x => x.TimeStamp > monday)
                .Sum(x => x.NumPackages);
            OnPropertyChanged(nameof(PackagesThisWeek));

            // Average Rate
            List<PackageRecord> recentlyPackedByUser = allPackages
                .Where(x => x.User == App.CurrentUser.UserName
                            && x.TimeStamp.Date.AddDays(14) > DateTime.Today)
                .OrderBy(x => x.TimeStamp)
                .ToList();

            if (recentlyPackedByUser.Count > 0)
            {
                TimeSpan totalTime = new();
                int packagesCounted = 0;

                List<DateTime> daysRecorded = recentlyPackedByUser
                    .Select(x => x.TimeStamp.Date)
                    .Distinct()
                    .ToList();

                for (int i = 0; i < daysRecorded.Count; i++)
                {
                    List<PackageRecord> packagesOnDay = recentlyPackedByUser
                        .Where(x => x.TimeStamp.Date == daysRecorded[i])
                        .ToList();
                    if (packagesOnDay.Count < 5)
                    {
                        continue;
                    }

                    if (daysRecorded[i].Date != DateTime.Today)
                    {
                        totalTime += packagesOnDay.Last().TimeStamp - packagesOnDay.First().TimeStamp;
                    }
                    else
                    {
                        totalTime += DateTime.Now - packagesOnDay.First().TimeStamp;
                    }
                    packagesCounted += packagesOnDay.Sum(x => x.NumPackages);
                }

                HourlyRate = packagesCounted / totalTime.TotalHours;
                HourlyRate = double.IsNaN(HourlyRate) ? 0 : HourlyRate;
                OnPropertyChanged(nameof(HourlyRate));
            }
        }

        private async void LoadData()
        {
            //allPackages = new();
            //Packages = new();
            allPackages = await FirebaseHelper.Read<PackageRecord>();
            Packages = allPackages.Where(x => x.TimeStamp.Date == DateTime.Today).ToList();
        }

        private bool InputsAreValid()
        {
            Brush errorBrush = (Brush)Application.Current.Resources["Red"];
            Brush okBrush = (Brush)Application.Current.Resources["Surface"];
            bool result = true;

            if (string.IsNullOrEmpty(OrderRef.Trim()))
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

        private async void RecordPackageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InputsAreValid())
            {
                return;
            }
            PackageRecord newRecord = new()
            {
                User = App.CurrentUser.UserName,
                User_FirstName = App.CurrentUser.FirstName,
                User_LastName = App.CurrentUser.LastName,
                TimeStamp = DateTime.Now,
                WorkStation = WorkStation,
                MachineName = Environment.MachineName,
                DomainUser = $"{Environment.UserDomainName}/{Environment.UserName}",
                NumPackages = int.Parse(QtyText.Trim()),
                OrderReference = OrderRef.ToUpper().Trim()
            };

            string newRecordId = await FirebaseHelper.Insert(newRecord);

            if (string.IsNullOrEmpty(newRecordId))
            {
                MessageBox.Show(
                    $"Failed to add to database.{Environment.NewLine}If this persists please contact and administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            newRecord.FirebaseId = newRecordId;
            RecordInLogs(newRecord);

            allPackages.Add(newRecord);
            Packages.Add(newRecord);
            Packages = new(Packages.Where(x => x.TimeStamp.Date == DateTime.Today));

            ClearInputs();
        }

        public void ClearInputs()
        {
            OrderRef = string.Empty;
            QtyText = string.Empty;
        }

        public static void RecordInLogs(PackageRecord r)
        {
            string record = $"{r.User}\t{r.StrTimeStamp}\t{r.WorkStation}\t{r.NumPackages}\t{r.OrderReference}\n";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\package_log.txt";
            File.AppendAllText(path, record);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NumbersOnly(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ListView listControl = RecordsListBox as ListView;
            if (listControl.SelectedValue is not PackageRecord selectedRecord)
            {
                return;
            }

            MessageBoxResult userConfirmation = MessageBox.Show(
                $"Are you sure you want to delete the record for {selectedRecord.OrderReference}?" +
                $"{Environment.NewLine}This cannot be undone.",
                "Confirm delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (userConfirmation != MessageBoxResult.Yes)
            {
                return;
            }

            bool deleted = await FirebaseHelper.Delete(selectedRecord);

            if (!deleted)
            {
                MessageBox.Show(
                    $"An error occurred trying to delete record." +
                    $"{Environment.NewLine}If the error persists please contact and administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            allPackages.Remove(selectedRecord);
            Packages.Remove(selectedRecord);
            Packages = new(Packages.Where(x => x.TimeStamp.Date == DateTime.Today));
        }

        private void RecordsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = RecordsListBox.SelectedValue != null;
        }
    }
}
