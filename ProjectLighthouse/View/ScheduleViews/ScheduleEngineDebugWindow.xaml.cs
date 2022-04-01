using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProjectLighthouse.View
{
    public partial class ScheduleEngineDebugWindow : Window, INotifyPropertyChanged
    {
        public SchedulingEngine Engine { get; set; }

        public SchedulingEngine.MachineSchedule C1_SCHEDULE { get; set; }
        public SchedulingEngine.MachineSchedule C2_SCHEDULE { get; set; }
        public SchedulingEngine.MachineSchedule C3_SCHEDULE { get; set; }
        public SchedulingEngine.MachineSchedule C4_SCHEDULE { get; set; }
        public ScheduleEngineDebugWindow(List<LatheManufactureOrder> orders, List<Lathe> lathes, List<BarStock> bar)
        {
            InitializeComponent();

            Engine = new();
            Engine.Orders = new(orders);
            Engine.Lathes = new(lathes);

            foreach (LatheManufactureOrder order in Engine.Orders)
            {
                order.Bar = bar.Find(x => x.Id == order.BarID);
            }

            Engine.MakeThreads();
            Engine.CategoriseThreads();
            Engine.CreateBlankSchedule();
            Engine.ScheduleRequirements();

            C1_SCHEDULE = Engine.MachineSchedules.Where(x => x.Lathe.Id == "C01").Single();
            C2_SCHEDULE = Engine.MachineSchedules.Where(x => x.Lathe.Id == "C02").Single();
            C3_SCHEDULE = Engine.MachineSchedules.Where(x => x.Lathe.Id == "C03").Single();
            C4_SCHEDULE = Engine.MachineSchedules.Where(x => x.Lathe.Id == "C04").Single();

            this.DataContext = this;

            OnPropertyChanged(nameof(Engine));
            OnPropertyChanged(nameof(C1_SCHEDULE));
            OnPropertyChanged(nameof(C2_SCHEDULE));
            OnPropertyChanged(nameof(C3_SCHEDULE));
            OnPropertyChanged(nameof(C4_SCHEDULE));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
