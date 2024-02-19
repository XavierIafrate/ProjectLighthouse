using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Commands.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLighthouse.View.Scheduling.Components
{
    public partial class DisplayAdvisory : UserControl
    {
        public ProductionSchedule.Advisory Advisory
        {
            get { return (ProductionSchedule.Advisory)GetValue(AdvisoryProperty); }
            set { SetValue(AdvisoryProperty, value); }
        }

        public static readonly DependencyProperty AdvisoryProperty =
            DependencyProperty.Register("Advisory", typeof(ProductionSchedule.Advisory), typeof(DisplayAdvisory), new PropertyMetadata(null, SetValues));



        public SelectScheduleItemCommand SelectItemCommand
        {
            get { return (SelectScheduleItemCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public static readonly DependencyProperty SelectItemCommandProperty =
            DependencyProperty.Register("SelectItemCommand", typeof(SelectScheduleItemCommand), typeof(DisplayAdvisory), new PropertyMetadata(null));



        private static void SetValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DisplayAdvisory control) return;
            if (control.Advisory is null) return;

            control.ItemNameTextBlock.Text = control.Advisory.Item.Name;
            control.MessageTextBlock.Text = ProductionSchedule.Advisory.GetText(control.Advisory.Type);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.SelectItemCommand?.Execute(this.Advisory.Item);
        }

        public DisplayAdvisory()
        {
            InitializeComponent();
        }
    }
}
