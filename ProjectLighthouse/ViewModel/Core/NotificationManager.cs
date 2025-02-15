﻿using Microsoft.Toolkit.Uwp.Notifications;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.View.Orders;
using ProjectLighthouse.ViewModel.Drawings;
using ProjectLighthouse.ViewModel.Helpers;
using ProjectLighthouse.ViewModel.Orders;
using ProjectLighthouse.ViewModel.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;

namespace ProjectLighthouse.ViewModel.Core
{
    public class NotificationManager : BaseViewModel
    {
        public Timer DataRefreshTimer { get; set; }
        private List<Notification> myNotifications;
        public List<User> Users { get; set; } = new();
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

            Users = DatabaseHelper.Read<User>()
                .Where(x => !x.IsBlocked && x.ReceivesNotifications && x.UserName != App.CurrentUser.UserName)
                .ToList();

            List<Permission> permissionsList = DatabaseHelper.Read<Permission>();
            for (int i = 0; i < Users.Count; i++)
            {
                Users[i].UserPermissions = permissionsList.Where(x => x.UserId == Users[i].Id).ToList();
            }

            MyNotifications = DatabaseHelper.Read<Notification>()
                .Where(x => x.TargetUser == App.CurrentUser.UserName)
                .ToList();

            DateTime cutoff = DateTime.Now.AddDays(-30);

            for (int i = 0; i < MyNotifications.Count; i++)
            {
                Notification n = MyNotifications[i];
                if (n.TimeStamp < cutoff)
                {
                    DatabaseHelper.Delete<Notification>(n);
                }
            }

            MyNotifications = MyNotifications
                .Where(x => x.TimeStamp > cutoff)
                .OrderByDescending(x => x.TimeStamp)
                .ToList();

            SetInterfaceVariables();
        }

        void SetInterfaceVariables()
        {
            List<Notification> visibleNots = GetVisibleNotifications(MyNotifications);

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

        private static List<Notification> GetVisibleNotifications(List<Notification> nots)
        {
            List<Notification> result = nots
                .Where(x => x.TimeStamp > DateTime.Now.AddDays(-5))
                .OrderByDescending(x => x.TimeStamp)
                .ToList();


            List<Notification> deduplicatedNots = new();

            for (int i = 0; i < result.Count; i++)
            {
                if (string.IsNullOrEmpty(result[i].ToastAction))
                {
                    deduplicatedNots.Add(result[i]);
                    continue;
                }

                if (!deduplicatedNots.Any(x => x.ToastAction == result[i].ToastAction))
                {
                    deduplicatedNots.Add(result[i]);
                }
            }

            return deduplicatedNots.OrderBy(x => x.Seen).ThenByDescending(x => x.TimeStamp).ToList();
        }

        private void CreateTimer()
        {
            DataRefreshTimer = new();

            DataRefreshTimer.Elapsed += Timer_Tick;
            DataRefreshTimer.Interval = 60 * 1000;
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

            MyNotifications = MyNotifications.Where(x => nots.Any(y => y.Id == x.Id)).ToList();

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

            SetInterfaceVariables();

            int visibleNotSeenNots = App.MainViewModel.NotCount;
            if (visibleNotSeenNots > 3 && multiToast)
            {
                string header = $"{visibleNotSeenNots:0} new notification" + (visibleNotSeenNots == 1 ? "" : "s");
                new ToastContentBuilder()
                       .AddText(header)
                       .AddHeroImage(new Uri($@"{App.AppDataDirectory}lib\renders\StartPoint.png"))
                       .AddText("Click here to see your inbox.")
                       .AddArgument("action", "showNotifications")
                       .Show();
            }
        }

        private static void RaiseToast(Notification notification)
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
                if (MyNotifications[i].Seen)
                {
                    continue;
                }

                MarkRead(MyNotifications[i]);
            }

            CheckForNotifications(false);
        }

        public static int? ParseToastArgs(string rawArgs)
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

            if (args.TryGetValue("id", out string value))
            {
                return int.Parse(value);
            }
            else
            {
                return null;
            }
        }

        private static void ExecuteToastArgs(KeyValuePair<string, string> arg)
        {
            if (arg.Key == "action")
            {
                ExecuteToastAction(arg.Value);
            }
        }

        public static void ExecuteToastAction(string action)
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
                App.MainViewModel.UpdateViewCommand.Execute("Requests");
                if (App.MainViewModel.SelectedViewModel is RequestViewModel requestViewModel)
                {
                    string targetRequest = action.Replace("viewRequest:", "");

                    requestViewModel.SelectedFilter = "All";

                    requestViewModel.SelectedRequest = requestViewModel.FilteredRequests.Find(x => $"{x.Id:0}" == targetRequest);
                }
            }
            else if (action.StartsWith("viewManufactureOrder:"))
            {
                App.MainViewModel.UpdateViewCommand.Execute("Orders");
                if (App.MainViewModel.SelectedViewModel is OrderViewModel orderViewModel)
                {
                    string targetOrder = action.Replace("viewManufactureOrder:", "");
                    orderViewModel.SelectedItem = orderViewModel.FilteredItems.Find(x => x.Name == targetOrder);
                    if (orderViewModel.SelectedItem == null)
                    {
                        orderViewModel.SearchString = targetOrder;
                    }
                }
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
                    TechnicalDrawingGroup? targetGroup = drawingBrowserVM.FilteredDrawingGroups.Find(x => x.Drawings.Any(y => y.Id.ToString("0") == targetDrawing));

                    if (targetGroup is null)
                    {
                        drawingBrowserVM.ShowOldGroups = true;
                        drawingBrowserVM.ShowRejected = true;

                        targetGroup = drawingBrowserVM.FilteredDrawingGroups.Find(x => x.Drawings.Any(y => y.Id.ToString("0") == targetDrawing));
                    }

                    if (targetGroup == null)
                    {
                        MessageBox.Show($"Drawing with internal ID '{targetDrawing}' cannot be found. It might have been deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    drawingBrowserVM.SelectedGroup = targetGroup;
                    drawingBrowserVM.SelectedDrawing = drawingBrowserVM.SelectedGroup.Drawings.Find(x => x.Id.ToString("0") == targetDrawing);
                }

            }

            App.MainViewModel.MainWindow.Activate();
        }

        public void EnsureMarkedRead(Notification notification)
        {
            if (notification.Seen)
            {
                return;
            }

            if (string.IsNullOrEmpty(notification.ToastAction))
            {
                MarkRead(notification);
            }
            else
            {
                MarkActionExecuted(notification.ToastAction);
            }

            CheckForNotifications(false);
        }

        public void DeleteNotification(Notification notification)
        {
            DatabaseHelper.Delete(notification);
            CheckForNotifications(false);
        }

        private void MarkActionExecuted(string action)
        {
            List<Notification> toMarkRead = MyNotifications.Where(x => x.ToastAction == action && !x.Seen).ToList();

            for (int i = 0; i < toMarkRead.Count; i++)
            {
                MarkRead(toMarkRead[i]);
            }
        }

        private static void MarkRead(Notification not)
        {
            not.Seen = true;
            not.SeenTimeStamp = DateTime.Now;
            DatabaseHelper.Update(not);
        }

        public static void NotifyHandledException(Exception exception)
        {
            MessageBox.Show($"An error was encountered and the procedure was aborted:{Environment.NewLine}{exception.Message}", "Handled Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void NotifyRequestApproved(Request request)
        {
            User userToNotify = Users.Find(x => x.GetFullName() == request.RaisedBy);

            if (userToNotify == null) return;

            Notification newNotification = new(
                to: userToNotify.UserName,
                from: App.CurrentUser.UserName,
                header: "Request Approved",
                toastAction: $"viewRequest:{request.Id}",
                body: $"Your request for {request.Description} has been approved. Please update Lighthouse with the Purchase Reference.");

            _ = DatabaseHelper.Insert(newNotification);
        }

        public void NotifyRequestRaised(Request request)
        {
            List<User> usersToNotify = Users.Where(x => x.HasPermission(PermissionType.ApproveRequest) && x.GetFullName() != request.RaisedBy).ToList();
            foreach (User user in usersToNotify)
            {
                Notification newNotification = new(
                    to: user.UserName,
                    from: App.CurrentUser.UserName,
                    header: "New Request",
                    body: $"{App.CurrentUser.FirstName} has raised a request for {request.Description}.",
                    toastAction: $"viewRequest:{request.Id}");

                _ = DatabaseHelper.Insert(newNotification);
            }

        }

        public static void NotifyOrderAssignment(LatheManufactureOrder order, string targetUsername, bool unassigned)
        {
            string title = unassigned ? $"Unassigned: {order.Name}" : $"Assigned: {order.Name}";
            string body = unassigned ? $"{App.CurrentUser.FirstName} has reassigned this order." : $"{App.CurrentUser.FirstName} has assigned this order to you.";

            Notification newNotification = new(
                to: targetUsername,
                from: App.CurrentUser.UserName,
                header: title,
                body: body,
                toastAction: $"viewManufactureOrder:{order.Name}");

            _ = DatabaseHelper.Insert(newNotification);
        }
    }
}
