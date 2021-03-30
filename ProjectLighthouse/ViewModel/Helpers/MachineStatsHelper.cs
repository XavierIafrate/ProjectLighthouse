using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class MachineStatsHelper
    {
        private static string IP_ADDRESS = "http://192.168.100.172:5000/current";

        public static MachineStatistics GetStats()
        {
            MachineStatistics stats = new MachineStatistics();

            XmlTextReader reader = new XmlTextReader(IP_ADDRESS);

            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "xml.txt")))
            //{
            string lastName = "";
            
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        //outputFile.WriteLine("Node Name," + reader.Name + ",");

                        while (reader.MoveToNextAttribute()) // Read the attributes.
                            //outputFile.WriteLine("Attribute," + reader.Name + "," + reader.Value);
                            switch (reader.Name)
                            {
                                case "creationTime":
                                    DateTime dateVal = new DateTime();

                                     if(DateTime.TryParse(reader.Value, out dateVal))
                                    {
                                        stats.DataTime = dateVal;
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to parse datetime");
                                    }
                                    break;
                                case "name":
                                    lastName = reader.Value;
                                    break;
                            };
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        //outputFile.WriteLine("Text Value," + reader.Value + ",");
                        int tmpInt = new int();
                        switch (lastName)
                        {
                            case "serial_number":
                                stats.SerialNumber = reader.Value;
                                break;
                            case "availability":
                                stats.Availability = reader.Value;
                                break;
                            case "program":
                                stats.Program = reader.Value;
                                break;
                            case "controller_mode":
                                stats.ControllerMode = reader.Value;
                                break;
                            case "controller_mode_override_dr":
                                stats.ControllerModeOverride = reader.Value;
                                break;
                            case "emergency_stop":
                                stats.EmergencyStop = reader.Value;
                                break;
                            case "part_count_all":
                                if(int.TryParse(reader.Value, out tmpInt))
                                {
                                    stats.PartCountAll = tmpInt;
                                }
                                break;
                            case "part_count_remaining":
                                if (int.TryParse(reader.Value, out tmpInt))
                                {
                                    stats.PartCountRemaining = tmpInt;
                                }
                                break;
                            case "part_count_target":
                                if (int.TryParse(reader.Value, out tmpInt))
                                {
                                    stats.PartCountTarget = tmpInt;
                                }
                                break;
                            case "process_timer_process":
                                if (int.TryParse(reader.Value, out tmpInt))
                                {
                                    stats.CycleTime = tmpInt;
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        //outputFile.WriteLine("EndElementName," + reader.Name + ",");
                        break;
                }
            }
            //}
            return stats;
        }
    }

    
}
