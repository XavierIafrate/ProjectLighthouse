using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LighthouseMonitoring.Model
{
    public class MtConnectData
    {
        public DateTime CreationDate { get; set; }


        [FromXml("active_axes_1", typeof(string))]
        public XmlElement? ActiveAxes { get; set; }

        [FromXml("block_1", typeof(string))]
        public XmlElement? Block { get; set; }

        [FromXml("execution_1", typeof(string))]
        public XmlElement? Execution { get; set; }

        [FromXml("line_number_absolute_1", typeof(int))]
        public XmlElement? LineNumber { get; set; }

        [FromXml("path_feedrate_override_prog_1", typeof(int))]
        public XmlElement? PathFeedrateOverride { get; set; }

        [FromXml("program", typeof(string))]
        public XmlElement? Program { get; set; }

        [FromXml("program_sub_1", typeof(string))]
        public XmlElement? SubProgram { get; set; }


        [FromXml("process_timer_process", typeof(int))]
        public XmlElement? ProcessTimer { get; set; }

        [FromXml("equipment_timer_loaded", typeof(int))]
        public XmlElement? EquipmentTimer { get; set; }


        [FromXml("serial_number", typeof(string))]
        public XmlElement? SerialNumber { get; set; }

        [FromXml("availability", typeof(string))]
        public XmlElement? Availability { get; set; }

        [FromXml("controller_mode", typeof(string))]
        public XmlElement? ControllerMode { get; set; }

        [FromXml("controller_mode_override_dr", typeof(string))]
        public XmlElement? ControllerModeOverride { get; set; }

        [FromXml("emergency_stop", typeof(string))]
        public XmlElement? EmergencyStop { get; set; }

        [FromXml("part_count_all", typeof(int))]
        public XmlElement? PartCountAll { get; set; }

        [FromXml("part_count_remaining", typeof(int))]
        public XmlElement? PartCountRemaining { get; set; }

        [FromXml("part_count_target", typeof(int))]
        public XmlElement? PartCountTarget { get; set; }

        public List<SystemMessage> SystemMessages { get; set; } = new();

        public class SystemMessage
        {
            public string NativeCode { get; set; }
            public DateTime TimeStamp { get; set; }
            public string Type { get; set; }
            public string Text { get; set; }

            public SystemMessage(string nativeCode, DateTime timeStamp, string type, string text)
            {
                char tab = '\u0009';

                NativeCode = nativeCode;
                TimeStamp = timeStamp;
                Type = type.ToUpper();
                Text = text.Trim().Replace(tab.ToString(), "");
            }
        }

        public class XmlElement
        {
            public string Name { get; set; }
            public DateTime TimeStamp { get; set; }
            public dynamic Value { get; set; }

            public XmlElement(string name, DateTime timeStamp, object value)
            {
                Name = name;
                TimeStamp = timeStamp;
                Value = value;
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class FromXml : Attribute
        {
            public string Name { get; set; }
            public Type ValueType { get; set; }

            public FromXml(string name, Type type)
            {
                Name = name;
                ValueType = type;
            }
        }

        public static Dictionary<FromXml, PropertyInfo> GetPropertyMap()
        {
            Dictionary<FromXml, PropertyInfo> d = new();


            IEnumerable<PropertyInfo> props = typeof(MtConnectData)
                .GetProperties()
                .Where(x => 
                    Attribute.IsDefined(x, typeof(FromXml))
                );

            for (int i = 0; i < props.Count(); i++)
            {
                PropertyInfo prop = props.ElementAt(i);
                FromXml info = prop.GetCustomAttribute<FromXml>()!;
                
                d.Add(info, prop);
            }

            return d;
        }
    }
}
