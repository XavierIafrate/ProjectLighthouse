using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using System.Windows;
using System.Windows.Controls;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayOptimisation : UserControl
    {
        public ProductionSchedule.Optimisation Optimisation
        {
            get { return (ProductionSchedule.Optimisation)GetValue(OptimisationProperty); }
            set { SetValue(OptimisationProperty, value); }
        }

        public static readonly DependencyProperty OptimisationProperty =
            DependencyProperty.Register("Optimisation", typeof(ProductionSchedule.Optimisation), typeof(DisplayOptimisation), new PropertyMetadata(null, SetValues));

        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayOptimisation), new PropertyMetadata(null));


        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayOptimisation control) return;
            if (control.Optimisation is null) return;

            control.OptimisationTypeTextBlock.Text = ProductionSchedule.Optimisation.GetText(control.Optimisation.Type);
            control.AffectedOrdersListBox.ItemsSource = control.Optimisation.AffectedItems;
            control.ImplementedIcon.Visibility = control.Optimisation.Implemented ? Visibility.Visible : Visibility.Collapsed;
        }

        public DisplayOptimisation()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (this.SelectItemCommand is null) return;
            if (!this.SelectItemCommand.CanExecute(null)) return;
            if (button.Content is not ScheduleItem item) return;

            this.SelectItemCommand?.Execute(item);
        }
    }
}
