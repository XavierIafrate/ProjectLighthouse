using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;

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
            if (App.CurrentUser is null) return;
            MyNotifications = DatabaseHelper.Read<Notification>().Where(n => n.TargetUser == App.CurrentUser.UserName && n.Seen).ToList();
            NotificationCount = myNotifications.Where(n => !n.Seen).Count();
            App.MainViewModel.Notifications = MyNotifications.Where(n => n.TimeStamp.AddDays(7) > DateTime.Now && n.Seen).OrderByDescending(x => x.TimeStamp).ToList();
            App.MainViewModel.NotCount = 0;
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
            App.MainViewModel.Notifications = MyNotifications.Where(not => not.TimeStamp.AddDays(7) > DateTime.Now).OrderByDescending(x => x.TimeStamp).ToList();
        }

        private void RaiseToast(Notification notification)
        {
            if (!string.IsNullOrEmpty(notification.ToastInlineImageUrl))
            {
                new ToastContentBuilder()
                       .AddText(notification.Header)
                       .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                       .AddText(notification.Body)
                       .AddArgument("action", notification.ToastAction ?? "")
                       .AddArgument("id", $"{notification.Id:0}")
                       .AddInlineImage(new Uri($"{App.AppDataDirectory}{notification.ToastInlineImageUrl}"))
                       .Show(x => { x.Tag = $"{notification.Id:0}"; });
            }
            else
            {
                new ToastContentBuilder()
                       .AddText(notification.Header)
                       .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
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

        public int? ParseToastArgs(string rawArgs)
        {
            string[] separated = rawArgs.Split(';');
            Dictionary<string, string> args = new();
            for (int i = 0; i < separated.Length; i++)
            {
                string[] kvp = separated[i].Split("=");
                if (kvp.Length == 2)
                {
                    args.Add(kvp[0], kvp[1]);
                }
            }
            for (int i = 0; i < args.Count; i++)
            {
                ExecuteToastArgs(args.ToList()[i]);
            }

            if (args.ContainsKey("id"))
            {
                return int.Parse(args["id"]);
            }
            else
            {
                return null;
            }
        }

        private void ExecuteToastArgs(KeyValuePair<string, string> arg)
        {
            if (arg.Key == "action")
            {
                ExecuteToastAction(arg.Value);
            }
        }

        public void ExecuteToastAction(string action)
        {
            if (action == null)
            {
                return;
            }


            int loadedWindows = Application.Current.Windows.Cast<Window>()
                                               .Where(win => win.IsVisible && !string.IsNullOrEmpty(win.Title)).Count();

            if (loadedWindows != 1)
            {
                return;
            }

            if (action.StartsWith("viewRequest:"))
            {
                App.MainViewModel.UpdateViewCommand.Execute("View Requests");
            }
            else if (action.StartsWith("viewManufactureOrder:"))
            {
                EditLMOWindow window = new(action.Replace("viewManufactureOrder:", ""), App.CurrentUser.CanUpdateLMOs);
                window.ShowDialog();
            }
        }

        public void EnsureMarkedRead(Notification notification)
        {
            if (notification.Seen)
            {
                return;
            }
            notification.Seen = true;
            notification.SeenTimeStamp = DateTime.Now;
            DatabaseHelper.Update(notification);
            CheckForNotifications();
        }


    }
}
