using ProjectLighthouse.Model.Material;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Administration
{
    public partial class ProductManagerView : UserControl
    {
        public ProductManagerView()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            grad.Visibility = scrollViewer.VerticalOffset == 0
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearButton.IsEnabled = !string.IsNullOrEmpty(searchBox.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Content is not KeyValuePair<MaterialInfo?, TimeModel> model) return;

            try
            {
                Clipboard.SetText(model.Value.ToString());
            }
            catch (Exception ex)
            {
                NotificationManager.NotifyHandledException(ex);
            }
        }
    }
}
