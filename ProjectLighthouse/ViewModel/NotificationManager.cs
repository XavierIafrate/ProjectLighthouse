using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace ProjectLighthouse.ViewModel
{
    public class NotificationManager : BaseViewModel
    {
        public Timer DataRefreshTimer { get; set; }
        private List<Notification> myNotifications;

        public List<Notification> MyNotifications
        {
            get { return myNotifications; }
            set
            {
                myNotifications = value;
            }
        }

        private int notificationCount;

        public int NotificationCount
        {
            get { return notificationCount; }
            set 
            { 
                notificationCount = value;
                OnPropertyChanged();
            }
        }

        public NotificationManager()
        {
            CreateTimer();
        }

        public void Initialise()
        {
            MyNotifications = DatabaseHelper.Read<Notification>().Where(x => x.TargetUser == App.CurrentUser.UserName && x.Seen).ToList();
        }

        private void CreateTimer()
        {
            DataRefreshTimer = new();

            DataRefreshTimer.Elapsed += Timer_Tick;
            DataRefreshTimer.Interval = 10 * 1000;
            DataRefreshTimer.Enabled = true;
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            CheckForNotifications();
        }

        private void CheckForNotifications()
        {
            List<Notification> nots = DatabaseHelper.Read<Notification>().Where(x => x.TargetUser == App.CurrentUser.UserName).ToList();
            for (int i = 0; i < nots.Count; i++)
            {
                if (!MyNotifications.Select(x => x.Id).ToArray().Contains(nots[i].Id))
                {
                    RaiseToast(nots[i]);
                    MyNotifications.Add(nots[i]);
                }
            }

            NotificationCount = myNotifications.Where(not => !not.Seen).Count();
            App.MainViewModel.NotCount = NotificationCount;
            App.MainViewModel.Notifications = MyNotifications.Where(not => !not.Seen).ToList();
        }

        private void RaiseToast(Notification notification)
        {
            if (!string.IsNullOrEmpty(notification.ToastInlineImageUrl))
            {
                new ToastContentBuilder()
                       .AddText(notification.Header)
                       .AddHeroImage(new Uri($@"{App.ROOT_PATH}lib\renders\StartPoint.png"))
                       .AddText(notification.Body)
                       .AddArgument("action", notification.ToastAction ?? "")
                       .AddInlineImage(new Uri($"{App.ROOT_PATH}{notification.ToastInlineImageUrl}"))
                       .Show(x => { x.Tag = $"{notification.Id:0}"; });
            }
            else
            {
                new ToastContentBuilder()
                       .AddText(notification.Header)
                       .AddHeroImage(new Uri($@"{App.ROOT_PATH}lib\renders\StartPoint.png"))
                       .AddText(notification.Body)
                       .AddArgument("action", notification.ToastAction ?? "")
                       .Show(x => { x.Tag = $"{notification.Id:0}"; });
            }
        }

        public void ReadAll()
        {
            for (int i = 0; i < MyNotifications.Count; i++)
            {
                if (!MyNotifications[i].Seen)
                {
                    MyNotifications[i].Seen = true;
                    MyNotifications[i].SeenTimeStamp = DateTime.Now;
                    DatabaseHelper.Update(MyNotifications[i]);
                }
            }

            CheckForNotifications();
        }
    }
}
