using DocumentFormat.OpenXml.Drawing.Charts;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Orders.Components
{
    public partial class LotsEditor : UserControl, INotifyPropertyChanged
    {
        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        public static readonly DependencyProperty ItemNameProperty =
            DependencyProperty.Register("ItemName", typeof(string), typeof(LotsEditor), new PropertyMetadata(string.Empty, SetValues));

        public int CycleTime
        {
            get { return (int)GetValue(CycleTimeProperty); }
            set { SetValue(CycleTimeProperty, value); }
        }

        public static readonly DependencyProperty CycleTimeProperty =
            DependencyProperty.Register("CycleTime", typeof(int), typeof(LotsEditor), new PropertyMetadata(0));

        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(LotsEditor), new PropertyMetadata(false));

        public ScheduleItem Order
        {
            get { return (ScheduleItem)GetValue(OrderProperty); }
            set { SetValue(OrderProperty, value); }
        }

        public static readonly DependencyProperty OrderProperty =
            DependencyProperty.Register("Order", typeof(ScheduleItem), typeof(LotsEditor), new PropertyMetadata(null));

        public List<Lot> Lots
        {
            get { return (List<Lot>)GetValue(LotsProperty); }
            set { SetValue(LotsProperty, value); }
        }

        public static readonly DependencyProperty LotsProperty =
            DependencyProperty.Register("Lots", typeof(List<Lot>), typeof(LotsEditor), new PropertyMetadata(new List<Lot>(), SetValues));

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<Lot> visibleLots;
        public List<Lot> VisibleLots
        {
            get { return visibleLots; }
            set { visibleLots = value; OnPropertyChanged(); }
        }

        private Lot? selectedLot;
        public Lot? SelectedLot
        {
            get { return selectedLot; }
            set { selectedLot = value; OnPropertyChanged(); }
        }


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not LotsEditor control) return;

            if (!string.IsNullOrEmpty(control.ItemName))
            {
                control.VisibleLots = control.Lots.Where(x => x.ProductName == control.ItemName).ToList();
            }
            else
            {
                control.VisibleLots = control.Lots.ToList();
            }
        }



        public LotsEditor()
        {
            InitializeComponent();
            this.EditControls.IsEnabled = false;
        }

        private void AddLotButton_Click(object sender, RoutedEventArgs e)
        {
            Lot newLot = new()
            {
                Date = DateTime.Now,
                AddedBy = App.CurrentUser.UserName,
                Order = Order.Name,
                FromMachine = Order.AllocatedMachine,
                CycleTime = CycleTime,
                ProductName = ItemName,
                AllowDelivery = true,
                DateProduced = DateTime.Now.Hour >= 12
                        ? DateTime.Today.AddHours(-12)
                        : DateTime.Today.AddHours(12),
            };

            if (Order.Lots.Count > 0)
            {
                newLot.MaterialBatch = Order.Lots.Last().MaterialBatch;
            }

            Order.Lots = Order.Lots.Append(newLot).ToList();

            SelectedLot = newLot;


            //NewLot.ValidateAll();
            //if (NewLot.HasErrors)
            //{
            //    return;
            //}

            //Order.Lots = Order.Lots.Append(NewLot).OrderBy(x => x.DateProduced).ToList();
            //Order.FinishedQuantity = Order.Lots.Where(x => x.IsAccepted).Sum(x => x.Quantity);
            //SetNewLot();
            //CalculateProductionData();
            //Order.TimeToComplete = Order.CalculateTimeToComplete();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;

            this.EditControls.IsEnabled = listBox.SelectedValue is not null;
        }
    }
}
