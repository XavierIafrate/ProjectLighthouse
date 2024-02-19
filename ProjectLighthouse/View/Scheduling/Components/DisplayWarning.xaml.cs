using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayWarning : UserControl
    {
        public ProductionSchedule.Warning Warning
        {
            get { return (ProductionSchedule.Warning)GetValue(WarningProperty); }
            set { SetValue(WarningProperty, value); }
        }

        public static readonly DependencyProperty WarningProperty =
            DependencyProperty.Register("Warning", typeof(ProductionSchedule.Warning), typeof(DisplayWarning), new PropertyMetadata(null, SetValues));



        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayWarning), new PropertyMetadata(null));



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayWarning control) return;
            if (control.Warning is null) return;

            control.ItemNameTextBlock.Text = control.Warning.Item.Name;
            control.MessageTextBlock.Text = ProductionSchedule.Warning.GetText(control.Warning.Type);
        }

        public DisplayWarning()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelectItemCommand?.Execute(this.Warning.Item);
        }
    }
}
