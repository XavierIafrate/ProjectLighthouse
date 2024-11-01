using ProjectLighthouse.Model.Deliveries;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View.Administration
{
    public partial class EditDeliveryItemWindow : Window, INotifyPropertyChanged
    {
        public bool SaveExit { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private DeliveryItem originalItem;


        private DeliveryItem item;
        public DeliveryItem Item
        {
            get { return item; }
            set { item = value; OnPropertyChanged(); }
        }


        public EditDeliveryItemWindow(DeliveryItem item)
        {
            InitializeComponent();

            this.originalItem = item;
            this.Item = item.Clone() as DeliveryItem;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Item.PurchaseOrderReference == originalItem.PurchaseOrderReference && item.ExportProductName == originalItem.ExportProductName)
            {
                Close();
                return;
            }

            try
            {
                Item.EditedAt = DateTime.Now;
                Item.EditedBy = App.CurrentUser.UserName;
                bool success = DatabaseHelper.Update(Item, throwErrs: true);

                if (!success)
                {
                    MessageBox.Show("Failed to update the delivery item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    SaveExit = true;
                    originalItem = Item;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to update the delivery item:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
