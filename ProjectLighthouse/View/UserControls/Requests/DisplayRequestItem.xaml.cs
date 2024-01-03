using ProjectLighthouse.Model.Requests;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectLighthouse.View.UserControls
{
    public partial class DisplayRequestItem : UserControl
    {
        public ICommand EditItemCommand
        {
            get { return (ICommand)GetValue(EditItemCommandProperty); }
            set { SetValue(EditItemCommandProperty, value); }
        }

        public static readonly DependencyProperty EditItemCommandProperty =
            DependencyProperty.Register("EditItemCommand", typeof(ICommand), typeof(DisplayRequestItem), new PropertyMetadata(null));

        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register("RemoveCommand", typeof(ICommand), typeof(DisplayRequestItem), new PropertyMetadata(null, SetRemoveButton));

        private static void SetRemoveButton(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequestItem control) return;
            control.RemoveButton.Visibility = control.RemoveCommand is not null ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(DisplayRequestItem), new PropertyMetadata(false, SetEdit));

        private static void SetEdit(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequestItem control) return;
            if (control.RequestItem is null) return;

            control.RequiredDatePicker.IsEnabled = control.CanEdit;
            control.QuantityTextBox.IsEnabled = control.CanEdit;
        }

        public RequestItem RequestItem
        {
            get { return (RequestItem)GetValue(RequestItemProperty); }
            set { SetValue(RequestItemProperty, value); }
        }

        public static readonly DependencyProperty RequestItemProperty =
            DependencyProperty.Register("RequestItem", typeof(RequestItem), typeof(DisplayRequestItem), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayRequestItem control) return;
            if (control.RequestItem is null) return;

            if (control.RequestItem.Item is not null)
            {
                control.ItemNameText.Text = control.RequestItem.Item.ProductName;
                control.RequestItem.Item.ValidateForOrder();

                control.MissingDataStatement.Visibility = control.RequestItem.Item.HasErrors ? Visibility.Visible : Visibility.Collapsed;

                control.EditButton.Visibility = control.RequestItem.Item.HasErrors && App.CurrentUser.HasPermission(Model.Core.PermissionType.CreateProducts)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                control.CopyProductNameButton.Visibility = Visibility.Visible;
            }
            else
            {
                control.ItemNameText.Text = control.RequestItem.ItemId == 0 ? "Unknown Item" : "ID: " + control.RequestItem.ItemId.ToString();
                control.CopyProductNameButton.Visibility = Visibility.Collapsed;
            }

                control.QuantityTextBox.Text = control.RequestItem.QuantityRequired == 0 ? "" : control.RequestItem.QuantityRequired.ToString();
            control.RequiredDatePicker.SelectedDate = control.RequestItem.DateRequired;
            control.RequiredDatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            control.RequiredDatePicker.DisplayDateEnd = DateTime.Today.AddYears(1);


        }

        public DisplayRequestItem()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RemoveCommand?.Execute(RequestItem);
        }

        private void RequiredDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RequestItem.DateRequired != RequiredDatePicker.SelectedDate)
            {
                RequestItem.DateRequired = RequiredDatePicker.SelectedDate;
                RequestItem.NotifyRequirementChanged();
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuantityTextBox.BorderBrush = (Brush)App.Current.Resources["Surface"];
            if (int.TryParse(QuantityTextBox.Text, out int value))
            {
                if (RequestItem.QuantityRequired != value)
                {
                    RequestItem.QuantityRequired = value;
                    RequestItem.NotifyRequirementChanged();
                }
            }
            else
            {
                if (RequestItem.QuantityRequired != 0)
                {
                    RequestItem.QuantityRequired = 0;
                    RequestItem.NotifyRequirementChanged();
                }
                QuantityTextBox.BorderBrush = (Brush)App.Current.Resources["Red"];
            }

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditItemCommand?.Execute(RequestItem);
        }

        private void CopyProductName_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RequestItem.Item!.ExportProductName ?? RequestItem.Item.ProductName);
        }
    }
}
