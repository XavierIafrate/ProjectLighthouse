using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LighthouseMonitoring.Model
{
    public class MachineData
    {
        public string MachineId { get; set; }
        public string MachineName { get; set; }

        public DateTime DataTime { get; set; }

        public List<string> SystemMessages { get; set; } = new();
        public List<string> Errors { get; set; } = new();


        public string? ActiveAxes { get; set; }
        public string? Block { get; set; }
        public string? Execution { get; set; }
        public int? LineNumber { get; set; }
        public int? PathFeedrateOverride { get; set; }
        public string? Program { get; set; }
        public string? SubProgram { get; set; }
        public int? ProcessTimer { get; set; }
        public int? EquipmentTimer { get; set; }
        public string? SerialNumber { get; set; }
        public string? Availability { get; set; }
        public string? ControllerMode { get; set; }
        public string? EmergencyStop { get; set; }
        public int? PartCountAll { get; set; }
        public int? PartCountRemaining { get; set; }
        public int? PartCountTarget { get; set; }

        public MachineStatus Status { get; set; }

        public MachineData(Lathe lathe)
        {
            MachineId = lathe.Id;
            MachineName = lathe.FullName;
        }

        public void PushData(MtConnectData data)
        {
            DataTime = data.CreationDate;

            ActiveAxes = data.ActiveAxes!.Value;
            Block = data.Block!.Value;
            Execution = data.Execution!.Value;
            LineNumber = data.LineNumber!.Value;
            PathFeedrateOverride = data.PathFeedrateOverride!.Value;
            Program = data.Program!.Value;
            SubProgram = data.SubProgram!.Value;
            ProcessTimer = data.ProcessTimer!.Value;
            EquipmentTimer = data.EquipmentTimer!.Value;
            SerialNumber = data.SerialNumber!.Value;
            Availability = data.Availability!.Value;
            ControllerMode = data.ControllerMode!.Value;
            EmergencyStop = data.EmergencyStop!.Value;
            PartCountAll = data.PartCountAll!.Value;
            PartCountRemaining = data.PartCountRemaining!.Value;
            PartCountTarget = data.PartCountTarget!.Value;

            SystemMessages.Clear();
            Errors.Clear();

            data.SystemMessages
                .ForEach(x => SystemMessages.Add($"{x.TimeStamp:s}\t{x.Type}\t{x.NativeCode}\t{x.Text}"));

            data.SystemMessages
                .Where(x => x.Type == "WARNING").ToList()
                .ForEach(x =>
                    Errors.Add($"{x.TimeStamp:s}\t{x.Type}\t{x.NativeCode}\t{x.Text}")
                    );

            SystemMessages = new(SystemMessages);
            Errors = new(Errors);

            if (data.ControllerMode!.Value == "AUTOMATIC")
            {
                Status = MachineStatus.Running;
            }
            else
            {
                Status = MachineStatus.Breakdown;
            }
        }
    }
}
