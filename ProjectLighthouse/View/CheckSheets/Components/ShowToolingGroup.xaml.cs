using ProjectLighthouse.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.CheckSheets.Components
{
    public partial class ShowToolingGroup : UserControl
    {
        public Product.ToolingGroup ToolingGroup
        {
            get { return (Product.ToolingGroup)GetValue(ToolingGroupProperty); }
            set { SetValue(ToolingGroupProperty, value); }
        }

        public static readonly DependencyProperty ToolingGroupProperty =
            DependencyProperty.Register("ToolingGroup", typeof(Product.ToolingGroup), typeof(ShowToolingGroup), new PropertyMetadata(null, SetValues));

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ShowToolingGroup control)
            {
                return;
            }
            control.DataContext = control.ToolingGroup;
        }

        public ShowToolingGroup()
        {
            InitializeComponent();
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Debug.WriteLine($"Row {e.Row} {e.EditAction}");
        }


        private void Grid_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            if (e.Command == DataGrid.DeleteCommand)
            {
                if (MessageBox.Show(string.Format("Would you like to delete {0}", (grid.SelectedItem as CheckSheetField).Param1), "Confirm Delete", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                    e.Handled = true;
            }
        }
    }
}
