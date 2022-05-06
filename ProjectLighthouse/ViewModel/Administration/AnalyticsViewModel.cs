using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Logistics;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ProjectLighthouse.ViewModel
{
    public class AnalyticsViewModel : BaseViewModel
    {
        #region Vars

        private AnalyticsHelper analytics;

        public AnalyticsHelper Analytics
        {
            get { return analytics; }
            set
            {
                analytics = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public DownloadPackingDataCommand DownloadPackingDataCommand { get; set; }
        public SendRuntimeReportCommand RuntimeReportCommand { get; set; }
        #endregion

        public AnalyticsViewModel()
        {
            Analytics = new();
            DownloadPackingDataCommand = new(this);
            RuntimeReportCommand = new(this);
        }

        public async void DownloadPackingData()
        {
            List<PackageRecord> records = await FirebaseHelper.Read<PackageRecord>();
            CSVHelper.WriteListToCSV(records, "packing_kpi");
        }

        public void SendRuntimeReport(bool test)
        {
            List<User> to = new();

            List<User> allUsers = DatabaseHelper.Read<User>();

            to.Add(allUsers.Find(x => x.UserName == "xav"));
            if (!test)
            {
                to.Add(allUsers.Find(x => x.UserName == "anthony"));
                to.Add(allUsers.Find(x => x.UserName == "richard"));
            }

            EmailHelper emailHelper = new();

            //emailHelper.SendDailyRuntimeReport(to, new AnalyticsHelper(), DateTime.Today.AddDays(-4).AddHours(6));
            //emailHelper.SendDailyRuntimeReport(to, new AnalyticsHelper(), DateTime.Today.AddDays(-3).AddHours(6));
            //emailHelper.SendDailyRuntimeReport(to, new AnalyticsHelper(), DateTime.Today.AddDays(-2).AddHours(6));
            emailHelper.SendDailyRuntimeReport(to, new AnalyticsHelper());
        }

        private static bool DateWithinRange(DateTime inputDate, DateTime startingDate)
        {
            double diff = (inputDate - startingDate).TotalHours;
            return diff >= 0 && diff < 24;
        }

        private static string GetTempPdfPath()
        {
            return System.IO.Path.GetTempFileName() + ".pdf";
        }

        public static int GetIso8601WeekOfYear(DateTime time) // shamelessly stolen
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Tuesday);
        }
    }
}
