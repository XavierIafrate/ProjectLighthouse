using ProjectLighthouse.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ProjectLighthouse.ViewModel.Helpers
{
    class MachineStatsHelper
    {
        private static string IP_ADDRESS = "http://192.168.100.172";

        public static async Task<List<MachineStatistics>> GetStats()
        {
            List<MachineStatistics> resultList = new List<MachineStatistics>();
            List<Lathe> lathes = DatabaseHelper.Read<Lathe>().ToList();

            foreach (var lathe in lathes)
            {
                MachineStatistics stats = new MachineStatistics()
                {
                    SystemMessages = ""
                };
                stats.MachineID = lathe.FullName;
                string url = IP_ADDRESS + lathe.ControllerReference + "/current";

                bool connected = await IsConnected(url);

                if (!connected)
                    break;

                using (XmlTextReader reader = new XmlTextReader(url))
                {
                    string lastName = "";
                    try
                    {
                        while (reader.Read())
                        {
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    while (reader.MoveToNextAttribute())
                                        switch (reader.Name)
                                        {
                                            case "creationTime":
                                                DateTime dateVal = new DateTime();
                                                if (DateTime.TryParse(reader.Value, out dateVal))
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
                                case XmlNodeType.Text:
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
                                        case "block_1":
                                            stats.Block = reader.Value;
                                            break;
                                        case "execution_1":
                                            stats.Execution = reader.Value;
                                            break;
                                        case "emergency_stop":
                                            stats.EmergencyStop = reader.Value;
                                            break;
                                        case "part_count_all":
                                            if (int.TryParse(reader.Value, out tmpInt))
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
                                        case "equipment_timer_loaded":
                                            if (int.TryParse(reader.Value, out tmpInt))
                                            {
                                                stats.CuttingTime = tmpInt;
                                            }
                                            break;
                                        case "system":
                                            stats.SystemMessages = stats.SystemMessages + reader.Value + ";";
                                            break;
                                    }
                                    break;
                                case XmlNodeType.EndElement:
                                    break;
                            }
                        }

                        stats.SetStatus(); // Set text to running, setting, breakdown, offline
                        resultList.Add(stats);


                    }
                    catch (XmlException e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
            return resultList;
        }

        public static async Task<bool> IsConnected(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 1000; // 1s

            Debug.WriteLine(string.Format("Timeout: {0}", request.Timeout));

            try
            {
                var response = (HttpWebResponse)await
                Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse,
                                                    request.EndGetResponse,
                                                    null);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
