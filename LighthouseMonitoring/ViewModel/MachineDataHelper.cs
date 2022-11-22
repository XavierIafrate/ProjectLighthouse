using LighthouseMonitoring.Model;
using MtconnectCore.Standard.Documents.Devices;
using MtconnectCore;
using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Windows.UI.WebUI;
using static SQLite.SQLite3;

namespace LighthouseMonitoring.ViewModel
{
    public static class MachineDataHelper
    {
        private static string IP_ADDRESS;

        public static void Initialise()
        {
            IP_ADDRESS = Settings1.Default.MTConnectHost;
        }

        public static MachineData GetMachineData(Lathe lathe)
        {
            if (string.IsNullOrEmpty(lathe.ControllerReference))
            {
                throw new ArgumentNullException($"{lathe.Id} - Controller reference is null");
            }


            //Uri uri = new Uri(@"C:\Users\xavie\Desktop\C01.xml");

            //// Initialize the Agent service that allows to easily send requests to the MTConnect Agent according to the MTConnect specification.
            //using (MtconnectAgentService mtcService = new(uri))
            //{
            //    // We'll send a PROBE request to the MTConnect Agent. Note that these requests are asynchronously and we're getting the result here synchronously.
            //    DevicesDocument mtcDocument = (DevicesDocument)mtcService.Probe().Result;

            //    // Verify the Response Document is what we think it is...
            //    if (mtcDocument is DevicesDocument)
            //    {
            //        // Here, we're using the Consoul library to cleanly display the Response Document as a stringified JSON in the Console window.
            //        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(mtcDocument), ConsoleColor.DarkGray);

            //        Console.WriteLine("Done!", ConsoleColor.Green);
            //    }
            //    else
            //    {
            //        Console.WriteLine($"Unexpected document type");
            //    }

            //}
                string url = IP_ADDRESS + lathe.ControllerReference + "/current";
            MachineData result = new(lathe);

            url = @"C:\Users\xavie\Desktop\C01.xml";
            using XmlTextReader reader = new(url);

            Dictionary<string, object> parsedData = ParseXmlData(reader);

            


            //catch (XmlException e)


            return result;


        }

        private static Dictionary<string, object> ParseXmlData(XmlReader reader)
        {
            Dictionary<string, object> result = new();
            string lastElement = "";

            while (reader.Read())
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Debug.WriteLine(reader.Name);
                        break;
                    //case XmlNodeType.Element:
                    //    //result = AddXmlElement(reader, result);
                    //    break;
                    //case XmlNodeType.Text:
                    //    int tmpInt = new();
                    //    switch (lastElement)
                    //    {
                    //        case "serial_number":
                    //            stats.SerialNumber = reader.Value;
                    //            break;
                    //        case "availability":
                    //            stats.Availability = reader.Value;
                    //            break;
                    //        case "program":
                    //            stats.Program = reader.Value;
                    //            break;
                    //        case "controller_mode":
                    //            stats.ControllerMode = reader.Value;
                    //            break;
                    //        case "block_1":
                    //            stats.Block = reader.Value;
                    //            break;
                    //        case "execution_1":
                    //            stats.Execution = reader.Value;
                    //            break;
                    //        case "emergency_stop":
                    //            stats.EmergencyStop = reader.Value;
                    //            break;
                    //        case "part_count_all":
                    //            if (int.TryParse(reader.Value, out tmpInt))
                    //            {
                    //                stats.PartCountAll = tmpInt;
                    //            }
                    //            break;
                    //        case "part_count_remaining":
                    //            if (int.TryParse(reader.Value, out tmpInt))
                    //            {
                    //                stats.PartCountRemaining = tmpInt;
                    //            }
                    //            break;
                    //        case "part_count_target":
                    //            if (int.TryParse(reader.Value, out tmpInt))
                    //            {
                    //                stats.PartCountTarget = tmpInt;
                    //            }
                    //            break;
                    //        case "process_timer_process":
                    //            if (int.TryParse(reader.Value, out tmpInt))
                    //            {
                    //                stats.CycleTime = tmpInt;
                    //            }
                    //            break;
                    //        case "equipment_timer_loaded":
                    //            if (int.TryParse(reader.Value, out tmpInt))
                    //            {
                    //                stats.CuttingTime = tmpInt;
                    //            }
                    //            break;
                    //        case "system":
                    //            stats.SystemMessages = stats.SystemMessages + reader.Value + ";";
                    //            //using (StreamWriter w = File.AppendText(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "system_messages_log.txt")))
                    //            //{
                    //            //    Log(reader.Value, w);
                    //            //}
                    //            break;
                    //    }
                    //    break;
                    //case XmlNodeType.EndElement:
                    //    break;
                }
            }

            return result;
        }


        //private static Dictionary<string, object> ParseXmlElement(XmlReader reader, Dictionary<string, object> result)
        //{
        //    while (reader.MoveToNextAttribute())
        //    {
        //        //dynamic value = 
        //        switch (reader.Name)
        //        {
        //            case "creationTime":
        //                //DateTime.Parse(reader.Value, out DateTime date)
        //                //                    result.DataTime = date;

        //                break;

        //            case "name":
        //                //lastElement = reader.Value;
        //                break;
        //        };
        //    }
        //}

    }
}
