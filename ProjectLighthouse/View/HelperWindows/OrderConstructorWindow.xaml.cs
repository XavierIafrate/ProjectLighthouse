using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View
{
    public partial class LMOContructorWindow : Window
    {
        public bool wasCancelled;
        public Request ApprovedRequest;
        public LatheManufactureOrder NewOrder;
        public List<LatheManufactureOrderItem> NewOrderItems;
        bool IgnoreRuntiumeCap;
        double TargetRuntime;

        BarStock OrderBar;
        public List<TurnedProduct> ListboxProducts;
        public TurnedProduct RequiredTurnedProduct;
        public List<TurnedProduct> ProductPool;

        public LMOContructorWindow(Request approvedRequest, double targetRuntime, List<TurnedProduct> allProducts)
        {
            InitializeComponent();
            TargetRuntime = targetRuntime;
            TargetRuntimeSlider.Value = TargetRuntime;
            ApprovedRequest = approvedRequest;
            wasCancelled = true;

            NewOrder = new LatheManufactureOrder()
            {
                ItemNeedsCleaning = ApprovedRequest.CleanCustomerRequirement
            };

            NewOrderItems = new List<LatheManufactureOrderItem>();
            
            List<BarStock> bars = DatabaseHelper.Read<BarStock>();

            RequiredTurnedProduct = allProducts.First(p => p.ProductName == approvedRequest.Product);

            ProductPool = RequiredTurnedProduct.isSpecialPart
                ? allProducts.Where(p => RequiredTurnedProduct.IsScheduleCompatible(p) && p.CanBeManufactured()).ToList()
                : allProducts.Where(p => RequiredTurnedProduct.IsScheduleCompatible(p) && p.CanBeManufactured() && !p.isSpecialPart).ToList();


            ListboxProducts = new List<TurnedProduct>(ProductPool);
            OrderBar = bars.First(b => b.Id == RequiredTurnedProduct.BarID);

            NewOrderItems = RequestsEngine.GetRecommendedOrderItems(ProductPool, 
                RequiredTurnedProduct, 
                ApprovedRequest.QuantityRequired, 
                TimeSpan.FromDays(targetRuntime), 
                approvedRequest.DateRequired);

            LMOItemsListBox.ItemsSource = NewOrderItems;

            requiredProductTextBlock.Text = RequiredTurnedProduct.ProductName;
            requiredQuantityTextBlock.Text = $"{approvedRequest.QuantityRequired:#,##0} pcs";
            requiredDateTextBlock.Text = $"Delivery required by {approvedRequest.DateRequired:dd/MM/yy}";

            RefreshView();
            CalculateInsights();
        }

        public void RefreshView()
        {
            ListboxProducts.Clear();
            string[] onOrder = new string[NewOrderItems.Count];

            for (int i = 0; i < NewOrderItems.Count; i++)
            {
                onOrder[i] = NewOrderItems[i].ProductName;
            }

            ListboxProducts = ProductPool.Where(p => !onOrder.Contains(p.ProductName)).ToList();

            poolListBox.ItemsSource = ListboxProducts;
            //poolListBox.IsEnabled = NewOrderItems.Count != 4;
            //AddButton.IsEnabled = NewOrderItems.Count != 4;
            RemoveButton.IsEnabled = NewOrderItems.Count != 1;
        }

        private void CalculateInsights()
        {
            int totaltime = 0;
            int requiredtime = 0;
            double bars = 0;

            foreach (LatheManufactureOrderItem item in NewOrderItems)
            {
                int cycleTime = item.CycleTime == 0 
                    ? 120
                    : item.CycleTime;

                totaltime += cycleTime * item.TargetQuantity;
                bars += item.TargetQuantity * (item.MajorLength + 2) / 2700;
                if (item.RequiredQuantity > 0)
                {
                    requiredtime += item.RequiredQuantity * cycleTime;
                }
            }

            bars = Math.Ceiling(bars);

            int costPerBar = OrderBar.Cost;
            double dblmaterialCost = Math.Round(Convert.ToDouble(costPerBar) / 100 * bars, 2);

            nBars.Text = $"{bars}";

            NewOrder.NumberOfBars = bars;
            materialCost.Text = $"£{Math.Round(dblmaterialCost, 0)}";
            reqTime.Text = $"{Math.Round(requiredtime / (double)86400, 2)} day(s)";
            totalTime.Text = $"{Math.Round(totaltime / (double)86400, 2)} day(s)";
            NewOrder.TimeToComplete = totaltime;
        }

        private static string GetNewMOName()
        {
            List<LatheManufactureOrder> orders = DatabaseHelper.Read<LatheManufactureOrder>();
            int nOrders = orders.Count;
            string strOrderNum = Convert.ToString(nOrders + 1);
            int orderNumLen = strOrderNum.Length;
            const string blank = "M00000";

            return blank[..(6 - orderNumLen)] + strOrderNum;
        }

        private List<TechnicalDrawing> FindDrawings()
        {
            List<TechnicalDrawing> drawings = DatabaseHelper.Read<TechnicalDrawing>();

            List<TechnicalDrawing> results = new();
            for (int i = 0; i < NewOrderItems.Count; i++)
            {
                if (NewOrderItems[i].IsSpecialPart)
                {
                    List<TechnicalDrawing> matches = drawings.Where(d => d.DrawingName == NewOrderItems[i].ProductName && !d.IsArchetype).OrderByDescending(d => d.Revision).ToList();
                    if (matches.Count == 0)
                    {
                        NewOrderItems[i].DrawingId = 0;
                    }
                    else
                    {
                        NewOrderItems[i].DrawingId = matches.First().Id;
                        results.Add(matches.First());
                    }
                }
                else
                {
                    List<TechnicalDrawing> matches = drawings.Where(d => d.DrawingName == NewOrderItems[i].ProductName && !d.IsArchetype).OrderByDescending(d => d.Revision).ToList();
                    if (matches.Count == 0)
                    {
                        TechnicalDrawing best = GetBestDrawingForProduct(
                            family: NewOrderItems[i].ProductName[..5],
                            group: RequiredTurnedProduct.ProductGroup, 
                            material: RequiredTurnedProduct.Material, 
                            drawings:drawings);

                        if (best == null)
                        {
                            NewOrderItems[i].DrawingId = 0;
                        }
                        else
                        {
                            NewOrderItems[i].DrawingId = best.Id;
                            results.Add(best);

                        }
                    }
                    else
                    {
                        NewOrderItems[i].DrawingId = matches.First().Id;
                        results.Add(matches.First());
                    }
                }
            }


            return results.Distinct().ToList();
        }

        public TechnicalDrawing? GetBestDrawingForProduct(string family, string group,string material, List<TechnicalDrawing> drawings)
        {
            List<TechnicalDrawing> matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family && d.ToolingGroup == group && d.MaterialConstraint == material).ToList();
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }

            matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family && d.ToolingGroup == group).ToList();
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }

            matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family).ToList();
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }
            return null;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            NewOrder.Name = GetNewMOName();
            NewOrder.CreatedAt = DateTime.Now;
            NewOrder.CreatedBy = App.CurrentUser.GetFullName();
            NewOrder.IsComplete = false;
            NewOrder.Status = "Problem";
            NewOrder.State = OrderState.Problem;
            NewOrder.IsReady = false;
            NewOrder.IsUrgent = false;
            NewOrder.HasProgram = false;
            NewOrder.HasStarted = false;
            NewOrder.BarIsAllocated = false;
            NewOrder.MajorDiameter = RequiredTurnedProduct.MajorDiameter;
            NewOrder.BarsInStockAtCreation = OrderBar.InStock;
            NewOrder.BarID = OrderBar.Id;

            List<TechnicalDrawing> drawings = FindDrawings();

            // Add order & items to database
            _ = DatabaseHelper.Insert(NewOrder);
            foreach (LatheManufactureOrderItem item in NewOrderItems)
            {
                item.NeedsCleaning = ApprovedRequest.CleanCustomerRequirement && item.RequiredQuantity > 0;
                item.AssignedMO = NewOrder.Name;
                item.AddedBy = App.CurrentUser.GetFullName();
                item.DateAdded = DateTime.Now;
                DatabaseHelper.Insert(item);
            };

            foreach (TechnicalDrawing drawing in drawings)
            {
                OrderDrawing o = new() { DrawingId = drawing.Id, OrderId = NewOrder.Name };
                DatabaseHelper.Insert(o);
            }

            MessageBox.Show($"Created {NewOrder.Name}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            wasCancelled = false;
            Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LMOItemsListBox.SelectedValue is not LatheManufactureOrderItem selectedLMOItem)
            {
                return;
            }

            if (selectedLMOItem.RequiredQuantity > 0)
            {
                _ = MessageBox.Show("This product is a requirement for the request!", "Cannot remove.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            NewOrderItems.Remove(selectedLMOItem);

            CapTime();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //if (NewOrderItems.Count >= 4)
            //{
            //    _ = MessageBox.Show("Max Items reached", "Order Full", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return;
            //}

            if (poolListBox.SelectedValue is not TurnedProduct selectedProduct)
            {
                return;
            }

            NewOrderItems.Add(new(selectedProduct));

            CapTime();
        }

        private void UpdateQty_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(UpdateQty.Text, out int j))
            {
                return;
            }

            List<LatheManufactureOrderItem> items = (List<LatheManufactureOrderItem>)LMOItemsListBox.ItemsSource;
            LatheManufactureOrderItem selected = (LatheManufactureOrderItem)LMOItemsListBox.SelectedValue;

            foreach (LatheManufactureOrderItem i in items)
            {
                if (i.ProductName == selected.ProductName)
                {
                    i.TargetQuantity = Math.Max(j, i.RequiredQuantity);
                }
            }

            LMOItemsListBox.ItemsSource = new List<LatheManufactureOrderItem>(items);
            CalculateInsights();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TargetRuntimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = sender as Slider;
            TargetRuntime = Math.Round(slider.Value);
            TargetRuntimeText.Text = $"Runtime Cap [{TargetRuntime:N0} days]";
            CapTime();
        }

        private void CapTime()
        {
            if (NewOrderItems == null)
            {
                return;
            }
            
            NewOrderItems = IgnoreRuntiumeCap 
                ? NewOrderItems.ToList()
                : RequestsEngine.CapQuantitiesForTimeSpan(NewOrderItems, TimeSpan.FromDays(TargetRuntime), true).ToList();

            LMOItemsListBox.ItemsSource = NewOrderItems;
            RefreshView();
            CalculateInsights();
        }

        private void Override_Checked(object sender, RoutedEventArgs e)
        {
            IgnoreRuntiumeCap = true;
            TargetRuntimeSlider.IsEnabled = false;
        }

        private void Override_Unchecked(object sender, RoutedEventArgs e)
        {
            IgnoreRuntiumeCap = false;
            TargetRuntimeSlider.IsEnabled = true;
            CapTime();
        }
    }
}
