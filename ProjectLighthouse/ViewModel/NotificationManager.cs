using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model;
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
        public List<User> users = new();
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
            if (App.CurrentUser.UserName is null) return;
            users = DatabaseHelper.Read<User>().ToList();

            MyNotifications = DatabaseHelper.Read<Notification>().Where(x => x.TargetUser == App.CurrentUser.UserName).ToList();

            SetInterfaceVariables();
        }

        void SetInterfaceVariables()
        {
            List<Notification> visibleNots = MyNotifications
                .Where(n => n.TimeStamp.AddDays(7) > DateTime.Now)
                .OrderByDescending(x => x.TimeStamp).ToList();
            App.MainViewModel.Notifications = visibleNots;
            if (visibleNots.Count == 0)
            {
                App.MainViewModel.NotCount = 0;
                App.MainViewModel.NoNotifications = true;
                App.MainViewModel.NoNewNotifications = true;
            }
            else
            {
                App.MainViewModel.NotCount = visibleNots.Where(x => !x.Seen).Count();
                App.MainViewModel.NoNotifications = false;
                App.MainViewModel.NoNewNotifications = App.MainViewModel.NotCount == 0;
            }
            
        }

        private void CreateTimer()
        {
            DataRefreshTimer = new();

            DataRefreshTimer.Elapsed += Timer_Tick;
            DataRefreshTimer.Interval = 30 * 1000;
            DataRefreshTimer.Enabled = true;
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            CheckForNotifications(false);
        }

        public void CheckForNotifications(bool multiToast)
        {
            if (App.CurrentUser == null) return;
            List<Notification> nots = DatabaseHelper.Read<Notification>().Where(x => x.TargetUser == App.CurrentUser.UserName && x.TimeStamp.AddDays(7) > DateTime.Now).ToList();

            int numNewNots = nots.Where(n => !n.Seen).Count();

            for (int i = 0; i < nots.Count; i++)
            {
                if (!MyNotifications.Select(x => x.Id).ToArray().Contains(nots[i].Id))
                {
                    if (numNewNots <= 3)
                    {
                        RaiseToast(nots[i]);
                    }
                    MyNotifications.Add(nots[i]);
                }
            }

            if (numNewNots > 3 && multiToast)
            {
                string header = $"{numNewNots:0} new notification" + ((numNewNots == 1) ? "" : "s");
                new ToastContentBuilder()
                       .AddText(header)
                       .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                       .AddText("Click here to see your inbox.")
                       .AddArgument("action", "showNotifications")
                       .Show();
            }

            SetInterfaceVariables();
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

            CheckForNotifications(false);
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
            else if (action == "showNotifications")
            {
                App.MainViewModel.NotificationsBarVis = Visibility.Visible;
            }
            else if (action.StartsWith("viewDrawing:"))
            {
                string targetDrawing = action.Replace("viewDrawing:", "");
                App.MainViewModel.UpdateViewCommand.Execute("Drawings");
                if (App.MainViewModel.SelectedViewModel is DrawingBrowserViewModel drawingBrowserVM)
                {
                    drawingBrowserVM.SelectedGroup = drawingBrowserVM.DrawingGroups.First(x => x.Drawings.Any(y => y.DrawingName == targetDrawing));
                }

            }
            if (action.StartsWith("viewQC:"))
            {
                App.MainViewModel.UpdateViewCommand.Execute("Quality Check");
                if (App.MainViewModel.SelectedViewModel is QualityCheckViewModel qcViewModel)
                {
                    qcViewModel.SearchTerm = action;
                }
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
            CheckForNotifications(false);
        }


    }
}
