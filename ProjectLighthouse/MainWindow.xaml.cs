using ProjectLighthouse.Model.Administration;
using ProjectLighthouse.View;
using ProjectLighthouse.ViewModel.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using DocumentFormat.OpenXml.Vml.Office;
using System;



#if DEBUG
using ProjectLighthouse.ViewModel.Helpers;
#endif

namespace ProjectLighthouse
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddVersionNumber()
        {
            if (App.CurrentUser == null)
            {
                return;
            }

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            Title = App.CurrentUser.Locale switch
            {
                "" => "Lighthouse",
                "Polish" => "Latarnia Morska",
                "Persian" => "فانوس دریایی",
                "Welsh" => "Goleudy",
                "Latvian" => "Bāka",
                _ => "Lighthouse"
            };

            Title += $" v.{versionInfo.FileVersion}";

#if DEBUG
            Title += $" - {DatabaseHelper.DatabasePath}";
#endif

            if (!File.Exists($"{App.ROOT_PATH}docs\\index.html"))
            {
                HelpButton.Visibility = Visibility.Collapsed;
                HelpButton_Mini.Visibility = Visibility.Collapsed;
            }

            DemoBanner.Visibility = App.DemoMode ? Visibility.Visible : Visibility.Collapsed;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is not null and T t)
                {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }

        }

        public void SelectButton(string buttonName)
        {
            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(main_menu))
            {
                button.IsChecked = (string)button.CommandParameter == buttonName;
            }

            foreach (ToggleButton button in FindVisualChildren<ToggleButton>(mini_button_container))
            {
                button.IsChecked = (string)button.CommandParameter == buttonName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            manage_database_button.Visibility =
                (App.CurrentUser.HasPermission(Model.Core.PermissionType.ViewReports) ||
                App.CurrentUser.HasPermission(Model.Core.PermissionType.ManageLathes) ||
                App.CurrentUser.HasPermission(Model.Core.PermissionType.EditMaterials) ||
                App.CurrentUser.HasPermission(Model.Core.PermissionType.EditUsers))
                ? Visibility.Visible : Visibility.Collapsed;

            LoggedInUserName.Text = App.CurrentUser.GetFullName();
            LoggedInUserRole.Text = App.CurrentUser.Role.ToString();
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AboutWindow window = new() { Owner = this };
            window.ShowDialog();
        }

        private void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogoColour.Visibility = Visibility.Visible;
            LogoColour_mini.Visibility = Visibility.Visible;
            LogoMono.Visibility = Visibility.Hidden;
            LogoMono_mini.Visibility = Visibility.Hidden;
        }

        private void Rectangle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogoColour.Visibility = Visibility.Hidden;
            LogoColour_mini.Visibility = Visibility.Hidden;
            LogoMono.Visibility = Visibility.Visible;
            LogoMono_mini.Visibility = Visibility.Visible;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustMenuWidth();
        }

        private void AdjustMenuWidth()
        {
            if (MainGrid.ActualWidth > 1350)
            {
                
                MainGrid.ColumnDefinitions[0].Width = new(Math.Min(MainGrid.ActualWidth * 0.2, App.CurrentUser.DefaultMenuWidth ?? 350), GridUnitType.Pixel);
            }
            else
            {
                MainGrid.ColumnDefinitions[0].Width = new(80, GridUnitType.Pixel);
            }
            SetMenuStyle();
        }

        bool dragging = false;
        private void SetMenuStyle()
        {
            double threshold = 220;
            if (dragging)
            {
                threshold = 100;
            }

            if (MainGrid.ColumnDefinitions[0].Width.Value > threshold)
            {
                MenuGrid.ColumnDefinitions[0].Width = new(0, GridUnitType.Star);
                MenuGrid.ColumnDefinitions[1].Width = new(1, GridUnitType.Star);
                if (!dragging)
                {
                    MainGrid.ColumnDefinitions[0].Width = new(MainGrid.ActualWidth * 0.2, GridUnitType.Pixel);
                    MainGrid.ColumnDefinitions[0].MaxWidth = Math.Min(350, App.CurrentUser.DefaultMenuWidth ?? 350);
                }
            }
            else
            {
                MenuGrid.ColumnDefinitions[0].Width = new(1, GridUnitType.Star);
                MenuGrid.ColumnDefinitions[1].Width = new(0, GridUnitType.Star);
                if (!dragging)
                {
                    MainGrid.ColumnDefinitions[0].Width = new(80, GridUnitType.Pixel);
                }
            }
        }


        

        private void GridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            User.PostDefaultMenuWidth(App.CurrentUser.UserName, MainGrid.ColumnDefinitions[0].ActualWidth);
            App.CurrentUser.DefaultMenuWidth = MainGrid.ColumnDefinitions[0].ActualWidth;
            dragging = false;
            Debug.WriteLine("Drag Completed");
            AdjustMenuWidth();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            App.ShowHelp(null);
        }

        private void GridSplitter_DragStarted(object sender, DragStartedEventArgs e)
        {
            dragging = true;
            MainGrid.ColumnDefinitions[0].MaxWidth = 350;
            Debug.WriteLine("Drag Started");
        }

        private void GridSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            SetMenuStyle();
        }
    }
}
