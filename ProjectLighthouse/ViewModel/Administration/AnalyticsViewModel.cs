using IO.ClickSend.ClickSend.Model;
using ProjectLighthouse.Model.Logistics;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;

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
            EmailHelper emailHelper = new();

            List<EmailRecipient> emailRecipients = new();
            emailRecipients.Add(new(email: "x.iafrate@wixroydgroup.com", name: "Xavier Iafrate"));
            if (!test)
            {
                emailRecipients.Add(new(email: "anthony.iafrate@automotioncomponents.com", name: "Anthony Iafrate"));
                emailRecipients.Add(new(email: "r.budd@wixroydgroup.com", name: "Richard Budd"));
                emailRecipients.Add(new(email: "m.iafrate@wixroyd.com", name: "Marcus Iafrate"));
                emailRecipients.Add(new(email: "a.harandizadeh@wixroydgroup.com", name: "Arian Harandizadeh"));
            }

            if (DateTime.Today.DayOfWeek == DayOfWeek.Monday)
            {
                emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper(), DateTime.Today.AddDays(-3).AddHours(6));
                emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper(), DateTime.Today.AddDays(-2).AddHours(6));
            }
            emailHelper.SendDailyRuntimeReport(emailRecipients, new AnalyticsHelper());
        }
    }
}
