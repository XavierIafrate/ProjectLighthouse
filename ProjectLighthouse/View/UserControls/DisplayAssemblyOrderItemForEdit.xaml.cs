using ProjectLighthouse.Model.Assembly;
using ProjectLighthouse.ViewModel.Commands;
using ProjectLighthouse.ViewModel.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectLighthouse.View.UserControls
{
    /// <summary>
    /// Interaction logic for DisplayAssemblyOrderItemForEdit.xaml
    /// </summary>
    public partial class DisplayAssemblyOrderItemForEdit : UserControl
    {
        private bool editing = false;

        #region DPs

        public AssemblyItemWithCommands Value
        {
            get { return (AssemblyItemWithCommands)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(AssemblyItemWithCommands), typeof(DisplayAssemblyOrderItemForEdit), new PropertyMetadata(null, SetValues));


        #endregion

        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayAssemblyOrderItemForEdit control)
                return;

            if (control.Value == null)
                return;

            if (control.Value.Child == null || control.Value.UpdateCommand == null)
                return;

            control.EditButton.Visibility = Visibility.Collapsed;
            control.UpdateUI();
        }

        public DisplayAssemblyOrderItemForEdit()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!editing)
                EditButton.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            EditButton.Visibility = Visibility.Collapsed;
            SavedText.Visibility = Visibility.Collapsed;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
            editing = true;
            QuantityReadyTextBox.Text = Value.Child.QuantityReady.ToString();
            QuantityReadyTextBox.Visibility = Visibility.Visible;
            ReadOnlyQuantityReadyText.Visibility = Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(QuantityReadyTextBox.Text))
            {
                MessageBox.Show("Enter a quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(QuantityReadyTextBox.Text, out int j))
            {
                Value.Child.QuantityReady = j;
                UpdateUI();
            }
            else
            {
                MessageBox.Show("Invalid Qty u muppet", "debug");
                return;
            }

            Value.UpdateCommand.Execute(Value.Child);

            SaveButton.Visibility = Visibility.Collapsed;
            SavedText.Visibility = Visibility.Visible;
            editing = false;
            QuantityReadyTextBox.Visibility = Visibility.Collapsed;
            ReadOnlyQuantityReadyText.Visibility = Visibility.Visible;
        }

        private void UpdateUI()
        {
            ProductNameTextBlock.Text = $"{Value.Child.ProductName}";
            ReadOnlyQuantityReadyText.Content = $"{Value.Child.QuantityReady:#,##0}";
            RequiredQuantityTextBlock.Text = $"{Value.Child.QuantityRequired:#,##0}";
            DoneCheckMark.Visibility = Value.Child.QuantityReady >= Value.Child.QuantityRequired
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void QuantityReadyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = TextBoxHelper.ValidateKeyPressNumbersOnly(e);
        }

        public class ItemWithUpdateCommand
        {
            public AssemblyOrderItem Item { get; set; }
            public UpdateAssemblyOrderItemCommand Command { get; set; }
        }
    }
}
