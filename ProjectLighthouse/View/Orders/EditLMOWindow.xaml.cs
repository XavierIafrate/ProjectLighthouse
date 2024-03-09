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


        public EditLMOWindow(string id, bool canEdit)
        {
            InitializeComponent();

            CanEdit = canEdit;
            LoadData(id);

            DataContext = this;
            UpdateControls(canEdit);
        }

        private void LoadData(string id)
        {
            ProductionStaff = DatabaseHelper.Read<User>()
                .Where(u => u.Role == UserRole.Production)
                .OrderBy(x => x.UserName)
                .Prepend(new() { UserName = null, FirstName = "Unassigned" })
                .ToList();

            Order = DatabaseHelper.Read<LatheManufactureOrder>().Find(x => x.Name == id);
            Lathes = DatabaseHelper.Read<Lathe>();

            if (Order is null) throw new Exception($"Cannot find order {id}");

            if (!string.IsNullOrEmpty(Order.TimeCodePlanned))
            {
                TmpTimeModel = new(Order.TimeCodePlanned);
            }

            if (BarStock is null)
            {
                // TODO handle bad inputs
                BarStock = DatabaseHelper.Read<BarStock>().Where(x => !x.IsDormant).ToList();
            }

            BarStock currentBar = BarStock.Find(x => x.Id == Order.BarID)
                ?? throw new Exception($"Cannot find bar {Order.BarID}");

            MaterialInfo material = DatabaseHelper.Read<MaterialInfo>().Find(m => m.Id == currentBar.MaterialId);
            currentBar.MaterialData = material;
            BarStock = BarStock.Where(x => x.MaterialId == currentBar.MaterialId).ToList();
            BarStock.ForEach(b => b.MaterialData = material);

            Order.Bar = currentBar;
            OnPropertyChanged(nameof(BarStock));

            savedOrder = (LatheManufactureOrder)Order.Clone();

            BarIssues = DatabaseHelper.Read<BarIssue>().Where(x => x.OrderId == Order.Name).ToList();
            Order.NumberOfBarsIssued = BarIssues.Sum(x => x.Quantity);
            TotalBarsText.Text = $"{Order.NumberOfBarsIssued}/{Math.Ceiling(Order.NumberOfBars)} Prepared";

            if (Order.StartDate.Date > DateTime.MinValue)
            {
                EndDatePicker.DisplayDateStart = Order.StartDate.Date.AddDays(1);
                EndDatePicker.DisplayDateEnd = Order.StartDate.Date.AddYears(1);
            }


            Items = DatabaseHelper.Read<LatheManufactureOrderItem>()
                .Where(x => x.AssignedMO == id)
                .OrderByDescending(n => n.RequiredQuantity)
                .ThenBy(n => n.ProductName)
                .ToList();

            //for (int i = 0; i < Items.Count; i++)
            //{
            //    Items[i].ShowEdit = App.CurrentUser.HasPermission(PermissionType.UpdateOrder);
            //}
            OnPropertyChanged(nameof(Items));


            Lots = DatabaseHelper.Read<Lot>().Where(x => x.Order == id).ToList();



            BreakdownCodes = DatabaseHelper.Read<BreakdownCode>();
            Breakdowns = DatabaseHelper.Read<MachineBreakdown>()
                .Where(x => x.OrderName == Order.Name)
                .OrderBy(x => x.BreakdownStarted)
                .ToList();

            NewBreakdown = new()
            {
                BreakdownStarted = DateTime.Now.AddHours(-1),
                BreakdownEnded = DateTime.Now,
                OrderName = Order.Name,
            };

            archetype = DatabaseHelper.Read<ProductGroup>().Find(x => x.Id == Order.GroupId)
                ?? throw new Exception($"Cannot find archetype {Order.GroupId}");

            product = DatabaseHelper.Read<Product>().Find(x => x.Id == archetype.ProductId);
        }




        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!CanEdit)
            {
                return;
            }

            if (TmpTimeModel is not null)
            {
                if (Order.TimeCodePlanned != TmpTimeModel.ToString())
                {
                    // reset if not saved
                    //Order.TimeModelPlanned = new(originalTimeCode);
                }
            }

            CalculateTimeAndBar();

            List<string> features = Order.Bar.MaterialData.RequiresFeaturesList;
            features.AddRange(Order.Bar.RequiresFeaturesList);
            features.AddRange(Order.Bar.MaterialData.RequiresFeaturesList);
            features.AddRange(archetype.RequiresFeaturesList);
            if (product is not null)
            {
                features.AddRange(product.RequiresFeaturesList);
            }

            features = features.OrderBy(x => x).Distinct().ToList();

            Order.RequiredFeaturesList = features;

            if (savedOrder.IsUpdated(Order))
            {
                SaveExit = true;
            }

            if (SaveExit && CanEdit)
            {
                _ = SaveOrder();
            }
        }

        private bool SaveOrder()
        {
            if (savedOrder.AssignedTo != Order.AssignedTo)
            {
                NotifyAssignmentChanged(savedOrder.AssignedTo, Order.AssignedTo);
            }

            Order.ModifiedBy = App.CurrentUser.GetFullName();

            DateTime modificationDate = DateTime.Now;
            Order.ModifiedAt = modificationDate;

            if (savedOrder.State < OrderState.Complete && Order.State >= OrderState.Complete)
            {
                Order.CompletedAt = modificationDate;
            }

            if (!DatabaseHelper.Update(Order))
            {
                MessageBox.Show("Failed to update the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void NotifyAssignmentChanged(string? from, string? to)
        {
            if (from is not null)
            {
                NotificationManager.NotifyOrderAssignment(Order, from, unassigned: true);
            }

            if (to is not null)
            {
                NotificationManager.NotifyOrderAssignment(Order, to, unassigned: false);
            }
        }


        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView list)
            {
                return;
            }

            if (list.SelectedValue is LatheManufactureOrderItem selectedItem)
            {
                RemoveItemButton.IsEnabled = App.CurrentUser.Role >= UserRole.Scheduling
                    && CanEdit
                    && Order.State < OrderState.Complete
                    && selectedItem.QuantityMade == 0;
            }
            else
            {
                RemoveItemButton.IsEnabled = false;
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            LatheManufactureOrderItem item = (LatheManufactureOrderItem)ItemsListBox.SelectedValue;

            if (item.RequiredQuantity > 0)
            {
                MessageBox.Show("Item has a customer requirement and cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Items.Count == 1)
            {
                MessageBox.Show("Order requires at least one item listed. Alternatively you can cancel the order.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult userChoice = MessageBox.Show($"Are you sure you want to delete {item.ProductName} from {Order.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (userChoice != MessageBoxResult.Yes)
            {
                return;
            }

            bool deleted = DatabaseHelper.Delete(item);

            if (!deleted)
            {
                MessageBox.Show("Failed to delete record from database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalculateTimeAndBar();
            SaveExit = true;
            SaveOrder();
            LoadData(Order.Name);
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddItemToOrderWindow window = new(Order.Id);
            if (window.PossibleItems.Count == 0)
            {
                MessageBox.Show("No further items are available to run on this order.", "Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Hide();
            window.ShowDialog();

            if (window.ItemsWereAdded)
            {
                SaveExit = true;
                CalculateTimeAndBar();
                SaveOrder();
                LoadData(Order.Name);
            }

            Show();
        }

       


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;
            if (listBox.SelectedValue is not BarIssue)
            {
                PrintIssueLabelButton.IsEnabled = false;
                return;
            }
            PrintIssueLabelButton.IsEnabled = true;
        }

        private void PrintIssueLabelButton_Click(object sender, RoutedEventArgs e)
        {
            if (BarIssuesListBox.SelectedValue == null) return;

            LabelPrintingHelper.PrintIssue((BarIssue)BarIssuesListBox.SelectedValue, copies: 2);
        }


        private void UpdateModelChanges_Click(object sender, RoutedEventArgs e)
        {
            if (TmpTimeModel is null) return;
            Order.TimeModelPlanned = TmpTimeModel;
            foreach (LatheManufactureOrderItem item in Items)
            {
                if (item.ModelledCycleTime is null)
                {
                    continue;
                }

                item.ModelledCycleTime = Order.TimeModelPlanned.At(item.MajorLength);
                DatabaseHelper.Update(item);
            }
            Items = new(Items);
            OnPropertyChanged(nameof(Items));

            CalculateTimeAndBar();

            ChartTimeModel();
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Order is null) return;

            if (EndDatePicker.SelectedDate is not null)
            {
                DateTime end = (DateTime)EndDatePicker.SelectedDate;
                end = end.ChangeTime(6, 0, 0, 0);

                Order.ScheduledEnd = end;
            }
            else
            {
                Order.ScheduledEnd = null;
            }
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Order.ScheduledEnd is null) return;
            DateTime dt = (DateTime)Order.ScheduledEnd;
            int hourCurrent = dt.Hour;
            hourCurrent++;
            hourCurrent %= 24;
            Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Order.ScheduledEnd is null) return;
            DateTime dt = (DateTime)Order.ScheduledEnd;
            int hourCurrent = dt.Hour;
            hourCurrent--;
            if (hourCurrent < 0)
            {
                hourCurrent = 23;
            }

            Order.ScheduledEnd = dt.ChangeTime(hourCurrent, 0, 0, 0);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Order.ScheduledEnd = null;
        }

        private void AddBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
            {
                MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.ValidateAll();
            if (NewBreakdown.HasErrors)
            {
                MessageBox.Show("New record has errors", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewBreakdown.BreakdownEnded > (Order.ScheduledEnd ?? Order.EndsAt()))
            {
                MessageBox.Show("Breakdown cannot end after order has finished", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewBreakdown.BreakdownStarted < Order.StartDate)
            {
                MessageBox.Show("Breakdown cannot begin before order has started", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            NewBreakdown.CreatedAt = DateTime.Now;
            NewBreakdown.CreatedBy = App.CurrentUser.UserName;

            try
            {
                DatabaseHelper.Insert(NewBreakdown, throwErrs: true);
            }
            catch
            {
                MessageBox.Show("Failed to insert to database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewBreakdown.BreakdownMeta = BreakdownCodes.Find(x => x.Id == NewBreakdown.BreakdownCode);
            Breakdowns = null;
            Breakdowns = DatabaseHelper.Read<MachineBreakdown>()
                .Where(x => x.OrderName == Order.Name)
                .OrderBy(x => x.BreakdownStarted)
                .ToList();
            OnPropertyChanged(nameof(Breakdowns));
            NewBreakdown = new()
            {
                BreakdownStarted = DateTime.Now.AddHours(-1),
                BreakdownEnded = DateTime.Now,
                OrderName = Order.Name
            };

            SaveExit = true;
        }

        private void removeBreakdownButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Tag is not int breakdownId) return;

            if (!App.CurrentUser.HasPermission(PermissionType.EditOrder))
            {
                MessageBox.Show("You do not have permissions to Edit Order", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MachineBreakdown? breakdown = Breakdowns.Find(x => x.Id == breakdownId);
            if (breakdown == null)
            {
                MessageBox.Show($"Cannot find breakdown with ID {breakdownId}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this breakdown record?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!DatabaseHelper.Delete(breakdown))
            {
                MessageBox.Show($"Failed to delete breakdown record", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Breakdowns = Breakdowns.Where(x => x.Id != breakdown.Id).ToList();
            OnPropertyChanged(nameof(Breakdowns));
            MessageBox.Show($"Deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
