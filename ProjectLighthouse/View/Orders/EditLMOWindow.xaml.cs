using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.Model.Core;
using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.View.UserControls;
using ProjectLighthouse.ViewModel.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.Orders
{
    public partial class EditLMOWindow : Window
    {


        public EditLMOWindow()
        {
            InitializeComponent();
        }





        private void Window_Closing(object sender, CancelEventArgs e)
        {
            
        }

        //private bool SaveOrder()
        //{
        //    //if (savedOrder.AssignedTo != Order.AssignedTo)
        //    //{
        //    //    NotifyAssignmentChanged(savedOrder.AssignedTo, Order.AssignedTo);
        //    //}

        //    return true;
        //}

        //private void NotifyAssignmentChanged(string? from, string? to)
        //{
        //    //if (from is not null)
        //    //{
        //    //    NotificationManager.NotifyOrderAssignment(Order, from, unassigned: true);
        //    //}

        //    //if (to is not null)
        //    //{
        //    //    NotificationManager.NotifyOrderAssignment(Order, to, unassigned: false);
        //    //}
        //}




        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            //LatheManufactureOrderItem item = (LatheManufactureOrderItem)ItemsListBox.SelectedValue;

            //if (item.RequiredQuantity > 0)
            //{
            //    MessageBox.Show("Item has a customer requirement and cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //if (Items.Count == 1)
            //{
            //    MessageBox.Show("Order requires at least one item listed. Alternatively you can cancel the order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //MessageBoxResult userChoice = MessageBox.Show($"Are you sure you want to delete {item.ProductName} from {Order.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //if (userChoice != MessageBoxResult.Yes)
            //{
            //    return;
            //}

            //bool deleted = DatabaseHelper.Delete(item);

            //if (!deleted)
            //{
            //    MessageBox.Show("Failed to delete record from database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //CalculateTimeAndBar();
            //SaveExit = true;
            //SaveOrder();
            //LoadData(Order.Name);
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            //AddItemToOrderWindow window = new(Order.Id);
            //if (window.PossibleItems.Count == 0)
            //{
            //    MessageBox.Show("No further items are available to run on this order.", "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}
            //Hide();
            //window.ShowDialog();

            //if (window.ItemsWereAdded)
            //{
            //    SaveExit = true;
            //    CalculateTimeAndBar();
            //    SaveOrder();
            //    LoadData(Order.Name);
            //}

            //Show();
        }

       


 

        //private void PrintIssueLabelButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (BarIssuesListBox.SelectedValue == null) return;

        //    //LabelPrintingHelper.PrintIssue((BarIssue)BarIssuesListBox.SelectedValue, copies: 2);
        //}


        //private void UpdateModelChanges_Click(object sender, RoutedEventArgs e)
        //{
        //    //if (TmpTimeModel is null) return;
        //    //Order.TimeModelPlanned = TmpTimeModel;
        //    //foreach (LatheManufactureOrderItem item in Items)
        //    //{
        //    //    if (item.ModelledCycleTime is null)
        //    //    {
        //    //        continue;
        //    //    }

        //    //    item.ModelledCycleTime = Order.TimeModelPlanned.At(item.MajorLength);
        //    //    DatabaseHelper.Update(item);
        //    //}
        //    //Items = new(Items);
        //    //OnPropertyChanged(nameof(Items));

        //    //CalculateTimeAndBar();

        //    //ChartTimeModel();
        //}

        //private void IncrementButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Order.ScheduledEnd is null) return;
        //    DateTime dt = (DateTime)Order.ScheduledEnd;
        //    int hourCurrent = dt.Hour;
        //    hourCurrent++;
        //    hourCurrent %= 24;
        //    Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        //}

        //private void DecrementButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Order.ScheduledEnd is null) return;
        //    DateTime dt = (DateTime)Order.ScheduledEnd;
        //    int hourCurrent = dt.Hour;
        //    hourCurrent--;
        //    if (hourCurrent < 0)
        //    {
        //        hourCurrent = 23;
        //    }

        //    Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        //}


        //private void AddBreakdownButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
        //    {
        //        MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    NewBreakdown.ValidateAll();
        //    if (NewBreakdown.HasErrors)
        //    {
        //        MessageBox.Show("New record has errors", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (NewBreakdown.BreakdownEnded > (Order.ScheduledEnd ?? Order.EndsAt()))
        //    {
        //        MessageBox.Show("Breakdown cannot end after order has finished", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (NewBreakdown.BreakdownStarted < Order.StartDate)
        //    {
        //        MessageBox.Show("Breakdown cannot begin before order has started", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }



        //    NewBreakdown.CreatedAt = DateTime.Now;
        //    NewBreakdown.CreatedBy = App.CurrentUser.UserName;

        //    try
        //    {
        //        DatabaseHelper.Insert(NewBreakdown, throwErrs: true);
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    NewBreakdown.BreakdownMeta = BreakdownCodes.Find(x => x.Id == NewBreakdown.BreakdownCode);
        //    Breakdowns = null;
        //    Breakdowns = DatabaseHelper.Read<MachineBreakdown>()
        //        .Where(x => x.OrderName == Order.Name)
        //        .OrderBy(x => x.BreakdownStarted)
        //        .ToList();
        //    OnPropertyChanged(nameof(Breakdowns));
        //    NewBreakdown = new()
        //    {
        //        BreakdownStarted = DateTime.Now.AddHours(-1),
        //        BreakdownEnded = DateTime.Now,
        //        OrderName = Order.Name
        //    };

        //    SaveExit = true;
        //}

        //private void removeBreakdownButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is not Button button) return;
        //    if (button.Tag is not int breakdownId) return;

        //    if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
        //    {
        //        MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    MachineBreakdown? breakdown = Breakdowns.Find(x => x.Id == breakdownId);
        //    if (breakdown == null)
        //    {
        //        MessageBox.Show($"Cannot find breakdown with ID {breakdownId}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (MessageBox.Show("Are you sure you want to delete this breakdown record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        //    {
        //        return;
        //    }

        //    if (!DatabaseHelper.Delete(breakdown))
        //    {
        //        MessageBox.Show($"Failed to delete breakdown record", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    Breakdowns = Breakdowns.Where(x => x.Id != breakdown.Id).ToList();
        //    OnPropertyChanged(nameof(Breakdowns));
        //    MessageBox.Show($"Deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //}
    }
}
