using Newtonsoft.Json;
using ProjectLighthouse.Model;
using System.Collections.Generic;
using System.IO;

namespace ProjectLighthouse.ViewModel.Helpers
{
    class MachineStatsHelper
    {
        //private static readonly string IP_ADDRESS = "http://192.168.103.102";

        public static List<MachineStatistics> GetStats()
        {
            try
            {
                string text = File.ReadAllText(Path.Join(App.ROOT_PATH, "lathes.json"));
                List<MachineStatistics> stats = JsonConvert.DeserializeObject<List<MachineStatistics>>(text);
                return stats;
            }
            catch
            {
                return null;
            }
        }
    }
}
